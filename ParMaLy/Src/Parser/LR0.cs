/*
 * Copyright (c) 2016, Ömercan Yazici <omercan AT pearcoding.eu>
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 *    1. Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *
 *    2. Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *
 *    3. Neither the name of the copyright owner may be used
 *       to endorse or promote products derived from this software without
 *       specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE
 */

using System.Collections.Generic;

namespace PML.Parser
{
    public class LR0 : BTParser
    {
        List<RuleState> _States = new List<RuleState>();
        RuleState _StartState;
        ActionTable _ActionTable = new ActionTable();
        GotoTable _GotoTable = new GotoTable();

        public List<RuleState> States { get { return _States; } }

        public RuleState StartState { get { return _StartState; } }

        public ActionTable ActionTable { get { return _ActionTable; } }

        public GotoTable GotoTable { get { return _GotoTable; } }

        public LR0()
        {
        }

        public void GenerateStates(Environment env, Logger logger)
        {
            _States.Clear();

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            var l = new List<RuleConfiguration>();
            foreach (Rule r in env.Start.Rules)
            {
                l.Add(new RuleConfiguration(r, -1));
            }
            StepState(env, l, null, null);
        }

        void GenerateState(Environment env, Rule r, RuleState state, int p)
        {
            RuleConfiguration conf = new RuleConfiguration(r, p);
            state.Configurations.Add(conf);
            
            if(!conf.IsLast)
            {
                RuleToken t = conf.GetNext();

                if (t.Type != RuleTokenType.Token)
                {
                    RuleGroup g = env.GroupByName(t.Name);

                    foreach (Rule r2 in g.Rules)
                    {
                        bool found = false;
                        foreach (RuleConfiguration c in state.Configurations)
                        {
                            if(c.Rule == r2 && c.Pos == 0)
                            {
                                found = true;
                                break;
                            }
                        }

                        if(!found)
                        {
                            GenerateState(env, r2, state, 0);
                        }
                    }
                }
            }
        }

        void StepState(Environment env, List<RuleConfiguration> confs, RuleState parent, RuleConfiguration producer)
        {
            RuleState state = new RuleState(_States.Count);

            foreach (RuleConfiguration c in confs)
            {
                if (!c.IsLast)
                {
                    GenerateState(env, c.Rule, state, c.Pos + 1);
                }
            }

            if (!_States.Contains(state))
            {
                if (parent != null)
                {
                    bool found = false;
                    foreach (var c in parent.Production)
                    {
                        if (c.State == state)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        RuleState.Connection c = new RuleState.Connection();
                        c.State = state;
                        c.Token = producer.GetNext();
                        parent.Production.Add(c);
                    }
                }
                else
                    _StartState = state;

                _States.Add(state);

                foreach (RuleConfiguration conf in state.Configurations)
                {
                    if (!conf.IsLast)
                    {
                        RuleToken t = conf.GetNext();

                        List<RuleConfiguration> next = new List<RuleConfiguration>();
                        foreach (RuleConfiguration c in state.Configurations)
                        {
                            if (!c.IsLast && c.GetNext().Type == t.Type && c.GetNext().Name == t.Name)
                                next.Add(c);
                        }

                        StepState(env, next, state, conf);
                    }
                }
            }
            else if (parent != null)
            {
                RuleState s2 = _States[_States.IndexOf(state)];
                bool found = false;
                foreach(var c in parent.Production)
                {
                    if (c.State == s2)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    RuleState.Connection c = new RuleState.Connection();
                    c.State = s2;
                    c.Token = producer.GetNext();
                    parent.Production.Add(c);
                }
            }
        }

        public void GenerateActionTable(Environment env, Logger logger)
        {
            _ActionTable.Clear();

            foreach(RuleState state in _States)
            {
                foreach(RuleConfiguration conf in state.Configurations)
                {
                    if(conf.Rule.Group == env.Start && conf.IsLast)
                    {
                        var a = _ActionTable.Get(state, null);
                        if (a != null && a.Action == ActionTable.Action.Accept)
                        {
                            logger.Log(LogLevel.Warning, "AcceptConflict (AC) in state " + state.ID);
                        }

                        _ActionTable.Set(state, null, ActionTable.Action.Accept, null);
                    }
                    else if(conf.IsLast)
                    {
                        foreach(string t in env.Tokens)
                        {
                            var a = _ActionTable.Get(state, t);
                            if (a != null && a.Action == ActionTable.Action.Shift && a.State != state)
                            {
                                if (a.Action != ActionTable.Action.Shift)
                                    logger.Log(LogLevel.Warning, "ReduceReduceConflict (RRC) in state " + state.ID
                                        + " with lookahead token " + (t == null ? "EOF" : "'" + t + "'"));
                                else
                                    logger.Log(LogLevel.Warning, "ShiftReduceConflict (SRC) in state " + state.ID
                                        + " with lookahead token " + (t == null ? "EOF" : "'" + t + "'"));
                            }

                            _ActionTable.Set(state, t, ActionTable.Action.Reduce, state);
                        }

                        var a2 = _ActionTable.Get(state, null);
                        if (a2 != null && a2.Action == ActionTable.Action.Shift && a2.State != state)
                        {
                            if (a2.Action != ActionTable.Action.Shift)
                                logger.Log(LogLevel.Warning, "ReduceReduceConflict (RRC) in state " + state.ID
                                    + " with lookahead token EOF");
                            else
                                logger.Log(LogLevel.Warning, "ShiftReduceConflict (SRC) in state " + state.ID
                                    + " with lookahead token EOF");
                        }

                        _ActionTable.Set(state, null, ActionTable.Action.Reduce, state);
                    }
                    else if(conf.GetNext().Type == RuleTokenType.Token)
                    {
                        RuleToken next = conf.GetNext();
                        RuleState found = null;
                        foreach(RuleState.Connection c in state.Production)
                        {
                            if(c.Token == next)
                            {
                                if (found != null)//Reduce Reduce Conflict
                                    logger.Log(LogLevel.Warning, "StateItemConflict (SIC) in state " + state.ID
                                    + " with next token '" + next.Name + "'");
                                else
                                    found = c.State;
                            }
                        }

                        var a = _ActionTable.Get(state, next.Name);
                        if (a != null && a.Action != ActionTable.Action.Shift && a.State != found)
                        {
                            if (a.Action != ActionTable.Action.Shift)
                                logger.Log(LogLevel.Warning, "ShiftReduceConflict (SRC) in state " + state.ID
                                    + " with next token '" + next.Name + "'");
                            else
                                logger.Log(LogLevel.Warning, "ShiftShiftConflict (SSC) in state " + state.ID
                                    + " with next token '" + next.Name + "'. Constructs RRC.");
                        }

                        _ActionTable.Set(state, next.Name, ActionTable.Action.Shift, found);
                    }
                }
            }
        }

        public void GenerateGotoTable(Environment env, Logger logger)
        {
            _GotoTable.Clear();

            foreach (RuleState state in _States)
            {
                foreach (var c in state.Production)
                {
                    if(c.Token.Type == RuleTokenType.Rule)
                    {
                        _GotoTable.Set(state, env.GroupByName(c.Token.Name), c.State);
                    }
                }
            }
        }
    }
}
