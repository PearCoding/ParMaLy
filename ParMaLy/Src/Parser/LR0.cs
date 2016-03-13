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
    public class LR0 : IBTParser
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
            _StartState = null;

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            RuleState state = new RuleState(_States.Count);
            foreach (Rule r in env.Start.Rules)
            {
                state.Header.Add(new RuleConfiguration(r, 0));
            }
            _StartState = state;
            _States.Add(state);

            Queue<RuleState> queue = new Queue<RuleState>();
            queue.Enqueue(state);

            while (queue.Count != 0)
            {
                var s = queue.Dequeue();
                System.Console.WriteLine("State ID: " + s.ID + " Queue: " + queue.Count + " left. Full state count: " + _States.Count);

                GenerateClosure(s, env, logger);
                StepState(s, queue, env, logger);
            }
        }

        void GenerateClosure(RuleState state, Environment env, Logger logger)
        {
            for (int i = 0; i < state.Count; ++i)
            {
                var c = state[i];
                if (!c.IsLast)
                {
                    var t = c.GetNext();

                    if (t.Type == RuleTokenType.Rule)
                    {
                        var grp = env.GroupByName(t.Name);

                        if (grp == null)
                        {
                            logger.Log(LogLevel.Error, "Unknown rule '" + t.Name + "' in state " + state.ID);
                        }
                        else
                        {
                            foreach (var r in grp.Rules)
                            {
                                RuleConfiguration conf2 = new RuleConfiguration(r, 0);
                                if (!state.Closure.Contains(conf2))
                                    state.Closure.Add(conf2);
                            }
                        }
                    }
                }
            }
        }

        void StepState(RuleState state, Queue<RuleState> queue, Environment env, Logger logger)
        {
            foreach (var c in state.All)
            {
                if (!c.IsLast)
                {
                    RuleState newState = new RuleState(_States.Count);
                    newState.Header.Add(new RuleConfiguration(c.Rule, c.Pos + 1));//c.Lookaheads

                    var t = c.GetNext();
                    foreach (var c2 in state.All)
                    {
                        if (!c2.IsLast && c2.GetNext().Type == t.Type && c2.GetNext().Name == t.Name)
                        {
                            var n = new RuleConfiguration(c2.Rule, c2.Pos + 1);
                            if (!newState.Header.Contains(n))
                                newState.Header.Add(n);
                        }
                    }

                    if (!_States.Contains(newState))//Check only header!
                    {
                        _States.Add(newState);

                        RuleState.Connection con = null;
                        foreach (var con2 in state.Production)
                        {
                            if (con2.State == newState && con2.Token == t)
                            {
                                con = con2;
                                break;
                            }
                        }

                        if (con == null)
                        {
                            con = new RuleState.Connection();
                            con.State = newState;
                            con.Token = t;
                            state.Production.Add(con);
                        }

                        queue.Enqueue(newState);
                    }
                    else
                    {
                        var oldState = _States[_States.IndexOf(newState)];

                        RuleState.Connection con = null;
                        foreach (var con2 in state.Production)
                        {
                            if (con2.State == oldState && con2.Token == t)
                            {
                                con = con2;
                                break;
                            }
                        }

                        if (con == null)
                        {
                            con = new RuleState.Connection();
                            con.State = oldState;
                            con.Token = t;
                            state.Production.Add(con);
                        }
                    }
                }
            }
        }

        public void GenerateActionTable(Environment env, Logger logger)
        {
            _ActionTable.Clear();

            foreach(RuleState state in _States)
            {
                foreach(RuleConfiguration conf in state.All)
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
