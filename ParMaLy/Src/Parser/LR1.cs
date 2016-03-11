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
    public class LR1 : BTParser
    {
        List<RuleState> _States = new List<RuleState>();
        RuleState _StartState;
        ActionTable _ActionTable = new ActionTable();
        GotoTable _GotoTable = new GotoTable();

        public List<RuleState> States { get { return _States; } }

        public RuleState StartState { get { return _StartState; } }

        public ActionTable ActionTable { get { return _ActionTable; } }

        public GotoTable GotoTable { get { return _GotoTable; } }

        public LR1()
        {
        }

        public void GenerateStates(Environment env, Logger logger)
        {
            _States.Clear();
            _StartState = null;

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            RuleState state = new RuleState(_States.Count);
            foreach (Rule r in env.Start.Rules)
            {
                state.Configurations.Add(new RuleConfiguration(r, 0, new RuleLookahead((string)null)));
            }
            _StartState = state;
            _States.Add(state);
            GenerateClosure(state, env, logger);
            StepState(state, env, logger);
        }

        void GenerateClosure(RuleState state, Environment env, Logger logger)
        {
            for (int i = 0; i < state.Configurations.Count; ++i)
            {
                var c = state.Configurations[i];
                if(!c.IsLast)
                {
                    var t = c.GetNext();

                    if(t.Type == RuleTokenType.Rule)
                    {
                        var grp = env.GroupByName(t.Name);

                        if(grp == null)
                        {
                            logger.Log(LogLevel.Error, "Unknown rule '" + t.Name + "' in state " + state.ID);
                        }
                        else
                        {
                            List<RuleToken> tmp;
                            int newp = c.Pos + 1;
                            if (c.Rule.Tokens.Count > newp)
                                tmp = c.Rule.Tokens.GetRange(newp, c.Rule.Tokens.Count - newp);
                            else
                                tmp = new List<RuleToken>();

                            List<string> delta = new List<string>();
                            foreach (var look in c.Lookaheads.Lookaheads)
                            {
                                var tmpL = new List<RuleToken>(tmp);
                                tmpL.Add(new RuleToken(c.Rule, RuleTokenType.Token, look[0]));//This is the point why we have LR(1) not LR(k)
                                var l = FirstSet.Generate(env, tmpL);
                                foreach(var s in l)//Really union?
                                {
                                    if (!delta.Contains(s))
                                        delta.Add(s);
                                }
                            }
                            
                            foreach (var r in grp.Rules)
                            {
                                RuleLookaheadSet set = new RuleLookaheadSet(delta.ToArray());

                                RuleConfiguration conf2 = new RuleConfiguration(r, 0, set);
                                if (!state.Configurations.Contains(conf2))
                                {
                                    state.Configurations.Add(conf2);
                                }
                            }
                        }
                    }
                }
            }
        }

        void StepState(RuleState state, Environment env, Logger logger)
        {
            foreach (var c in state.Configurations)
            {
                if (!c.IsLast)
                {
                    RuleState newState = new RuleState(_States.Count);
                    newState.Configurations.Add(new RuleConfiguration(c.Rule, c.Pos + 1, c.Lookaheads));

                    var t = c.GetNext();
                    foreach (var c2 in state.Configurations)
                    {
                        if (!c2.IsLast && c2.GetNext().Type == t.Type && c2.GetNext().Name == t.Name &&
                            c.Lookaheads == c2.Lookaheads)
                        {
                            var n = new RuleConfiguration(c2.Rule, c2.Pos + 1, c2.Lookaheads);
                            if (!newState.Configurations.Contains(n))
                                newState.Configurations.Add(n);
                        }
                    }

                    GenerateClosure(newState, env, logger);

                    if (!_States.Contains(newState))
                    {
                        _States.Add(newState);

                        var con = new RuleState.Connection();
                        con.State = newState;
                        con.Token = t;
                        state.Production.Add(con);

                        StepState(newState, env, logger);
                    }
                    else
                    {
                        var oldState = _States[_States.IndexOf(newState)];
                        var con = new RuleState.Connection();
                        con.State = oldState;
                        con.Token = t;
                        state.Production.Add(con);
                    }
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
                        _ActionTable.Set(state, null, ActionTable.Action.Accept, null);
                    }
                    else if(conf.IsLast)
                    {
                        foreach(string t in env.Tokens)
                        {
                            _ActionTable.Set(state, t, ActionTable.Action.Reduce, state);
                        }
                        _ActionTable.Set(state, null, ActionTable.Action.Reduce, state);
                    }
                    else if(conf.GetNext().Type == RuleTokenType.Token)
                    {
                        RuleToken next = conf.GetNext();
                        RuleState found = null;
                        foreach(RuleState.Connection c in state.Production)//TODO
                        {
                            if(c.Token == next)
                            {
                                if (found != null)//Reduce Reduce Conflict
                                    logger.Log(LogLevel.Warning, "ReduceReduceConflict in state " + state.ID);
                                else
                                    found = c.State;
                            }
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
