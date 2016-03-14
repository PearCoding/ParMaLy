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
using System.Linq;
    using System.Diagnostics;

namespace PML.Parser
{
    using Statistics;

    public class LALR1 : IBUParser
    {
        List<RuleState> _States = new List<RuleState>();
        RuleState _StartState;
        ActionTable _ActionTable = new ActionTable();
        GotoTable _GotoTable = new GotoTable();
        Statistics _Statistics;

        public List<RuleState> States { get { return _States; } }

        public RuleState StartState { get { return _StartState; } }

        public ActionTable ActionTable { get { return _ActionTable; } }

        public GotoTable GotoTable { get { return _GotoTable; } }

        public Statistics Statistics { get { return _Statistics; } }

        public LALR1()
        {
        }

        public void GenerateStates(Environment env, Logger logger)
        {
            _States.Clear();
            _StartState = null;
            _Statistics = new Statistics();
            _Statistics.BU = new BUStatistics();

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            RuleState state = new RuleState(_States.Count);
            foreach (Rule r in env.Start.Rules)
            {
                state.Header.Add(new RuleConfiguration(r, 0, new RuleLookahead((string)null)));
            }
            _StartState = state;
            _States.Add(state);

            Queue<RuleState> queue = new Queue<RuleState>();
            queue.Enqueue(state);

            Stopwatch watch = new Stopwatch();
            while (queue.Count != 0)
            {
                var s = queue.Dequeue();
                BUStatistics.ProcessEntry process = new BUStatistics.ProcessEntry(s, _States.Count, queue.Count);

                System.Console.WriteLine("State ID: " + s.ID + " Queue: " + queue.Count + " left. Full state count: " + _States.Count);

                watch.Start();
                GenerateClosure(s, logger);
                StepState(s, queue, logger);
                watch.Stop();

                process.TimeElapsed = watch.ElapsedMilliseconds;
                _Statistics.TimeElapsed += process.TimeElapsed;
                _Statistics.BU.Proceedings.Add(process);
            }
        }

        void GenerateClosure(RuleState state, Logger logger)//Same as LR1
        {
            for (int i = 0; i < state.Count; ++i)
            {
                var c = state[i];
                if(!c.IsLast)
                {
                    var t = c.GetNext();

                    if(t.Type == RuleTokenType.Rule)
                    {
                        List<RuleToken> tmp;
                        int newp = c.Pos + 1;
                        if (c.Rule.Tokens.Count > newp)
                            tmp = c.Rule.Tokens.GetRange(newp, c.Rule.Tokens.Count - newp);
                        else
                            tmp = new List<RuleToken>();
                            
                        foreach (var look in c.Lookaheads.Lookaheads)
                        {
                            var tmpL = new List<RuleToken>(tmp);
                            tmpL.Add(new RuleToken(c.Rule, RuleTokenType.Token, look[0]));//This is the point why we have LR(1) not LR(k)
                            var delta = FirstSet.Generate(tmpL);
                                                            
                            foreach (var r in t.Group.Rules)
                            {
                                RuleLookaheadSet set = new RuleLookaheadSet(delta);
                                RuleConfiguration conf2 = new RuleConfiguration(r, 0, set);

                                RuleConfiguration other = null;
                                foreach(var c2 in state.All)
                                {
                                    if(c2.Rule == r && c2.Pos == 0 && !object.ReferenceEquals(c2, c))
                                    {
                                        other = c2;
                                        break;
                                    }
                                }

                                if ((object)other != null)
                                    other.Lookaheads.AddUnique(set);
                                else
                                    state.Closure.Add(conf2);
                            }
                        }
                    }
                }
            }
        }

        void StepState(RuleState state, Queue<RuleState> queue, Logger logger)
        {
            foreach (var c in state.All)
            {
                if (!c.IsLast)
                {
                    RuleState newState = new RuleState(_States.Count);
                    newState.Header.Add(new RuleConfiguration(c.Rule, c.Pos + 1, c.Lookaheads));//c.Lookaheads

                    var t = c.GetNext();
                    foreach (var c2 in state.All)
                    {
                        if (!c2.IsLast && c2.GetNext().Type == t.Type && c2.GetNext().Name == t.Name /*&&
                            c.Lookahead == c2.Lookahead*/ /*c.Lookaheads == c2.Lookaheads*/)
                        {
                            var n = new RuleConfiguration(c2.Rule, c2.Pos + 1, c2.Lookaheads);//c. Lookaheads
                            if (!newState.Header.Contains(n))
                                newState.Header.Add(n);
                        }
                    }

                    // The merge state, which is different to LR(1)
                    RuleState mergeState = null;
                    foreach (var s in _States)
                    {
                        if (s.SemiEquals(newState))
                        {
                            mergeState = s;
                            break;
                        }
                    }

                    if (mergeState == null)//Didn't found anything... Add new
                    {
                        _States.Add(newState);

                        RuleState.Connection con = null;
                        foreach (var con2 in state.Production)
                        {
                            if(con2.State == newState && con2.Token == t)
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
                        if(mergeState.Closure.Count != 0)
                            GenerateClosure(newState, logger);

                        foreach (var c2 in newState.All)
                        {
                            foreach(var c3 in mergeState.All)
                            {
                                if(c2.SemiEquals(c3))
                                {
                                    foreach(var l in c2.Lookaheads)
                                    {
                                        if (!c3.Lookaheads.Contains(l))
                                            c3.Lookaheads.Add(l);
                                    }
                                }
                            }
                        }

                        RuleState.Connection con = null;
                        foreach (var con2 in state.Production)
                        {
                            if (con2.State == mergeState && con2.Token == t)
                            {
                                con = con2;
                                break;
                            }
                        }

                        if (con == null)
                        {
                            con = new RuleState.Connection();
                            con.State = mergeState;
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
                    if(conf.Rule.Group == env.Start &&
                        conf.IsLast &&
                        conf.Lookaheads.Contains((string)null))//Accept
                    {
                        var a = _ActionTable.Get(state, null);
                        if (a != null && a.Action != ActionTable.Action.Accept)
                            Statistics.BU.Conflicts.Add(new BUStatistics.ConflictEntry(BUStatistics.ConflictType.Accept, state));

                        _ActionTable.Set(state, null, ActionTable.Action.Accept, null);
                    }
                    else if(conf.IsLast)//Reduce
                    {
                        foreach (var l in conf.Lookaheads.Lookaheads)
                        {
                            var a = _ActionTable.Get(state, l[0]);
                            if (a != null)
                            {
                                if (a.Action != ActionTable.Action.Shift && a.State != state)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ReduceReduce, state, l[0]));
                                else if(a.Action == ActionTable.Action.Shift)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ShiftReduce, state, l[0]));
                            }

                            _ActionTable.Set(state, l[0], ActionTable.Action.Reduce, state);
                        }
                    }
                    else if(conf.GetNext().Type == RuleTokenType.Token)//Shift
                    {
                        RuleToken next = conf.GetNext();
                        RuleState found = null;
                        foreach(RuleState.Connection c in state.Production)
                        {
                            if(c.Token == next)
                            {
                                if (found != null)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.Internal, state, next.Name));
                                else
                                    found = c.State;
                            }
                        }

                        var a = _ActionTable.Get(state, next.Name);
                        if (a != null)
                        {
                            if (a.Action != ActionTable.Action.Shift)
                                Statistics.BU.Conflicts.Add(
                                    new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ShiftReduce, state, next.Name));
                            else if(a.Action == ActionTable.Action.Shift && a.State != found)
                                Statistics.BU.Conflicts.Add(
                                    new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ShiftShift, state, next.Name));
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
                        _GotoTable.Set(state, c.Token.Group, c.State);
                    }
                }
            }
        }

        public void Generate(Environment env, Logger logger)
        {
            GenerateStates(env, logger);
            GenerateActionTable(env, logger);
            GenerateGotoTable(env, logger);
        }
    }
}
