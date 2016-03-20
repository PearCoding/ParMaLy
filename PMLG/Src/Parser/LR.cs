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
using System.Diagnostics;
using System.Linq;

namespace PML.Parser
{
    using Statistics;
    using BU;

    public class LR : IBUParser
    {
        List<RuleState> _States = new List<RuleState>();
        RuleState _StartState;
        ActionTable _ActionTable = new ActionTable();
        GotoTable _GotoTable = new GotoTable();
        Statistics _Statistics;

        public string Name { get { return "LR(" + K + ")"; } }

        int _K = 1;
        public int K { get { return _K; } }

        public List<RuleState> States { get { return _States; } }

        public RuleState StartState { get { return _StartState; } }

        public ActionTable ActionTable { get { return _ActionTable; } }

        public GotoTable GotoTable { get { return _GotoTable; } }

        public Statistics Statistics { get { return _Statistics; } }

        public LR(int k = 1)
        {
            _K = k;
        }

        void GenerateStates(Environment env, Logger logger)
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
                state.Header.Add(new RuleConfiguration(r, 0, env.FollowCache.Get(env.Start, K)));
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
                GenerateClosure(s, env.FirstCache, logger);
                StepState(s, queue, env.FirstCache, logger);
                watch.Stop();

                process.TimeElapsed = watch.ElapsedMilliseconds;
                _Statistics.TimeElapsed += process.TimeElapsed;
                _Statistics.BU.Proceedings.Add(process);
            }
        }

        void GenerateClosure(RuleState state, FirstSetCache firstCache, Logger logger)
        {
            int i = 0;
            while(i >= 0)
            {
                var c = state[i];
                c.Dirty = false;
                if (!c.IsLast)
                {
                    var t = c.GetNext();

                    if(t.Type == RuleTokenType.Rule)
                    {
                        IEnumerable<RuleToken> tmp;
                        int newp = c.Pos + 1;
                        if (c.Rule.Tokens.Count > newp)
                            tmp = c.Rule.Tokens.Skip(newp);
                        else
                            tmp = new List<RuleToken>();
                            
                        for (int j = 0; j < c.Lookaheads.Count(); ++j)
                        {
                            var look = c.Lookaheads[j];
                            var delta = firstCache.Generate(
                                look == null ? tmp : tmp.Concat(look.Take(System.Math.Min(look.Count, K))),
                                K);
                                                            
                            foreach (var r in t.Group.Rules)
                            {
                                RuleLookaheadSet set = new RuleLookaheadSet(delta);
                                RuleConfiguration conf2 = new RuleConfiguration(r, 0, set);

                                // Grouping algorithm
                                RuleConfiguration other = null;
                                foreach(var c2 in state.All)
                                {
                                    if(c2.SemiEquals(conf2))
                                    {
                                        other = c2;
                                        break;
                                    }
                                }

                                if ((object)other != null)
                                {
                                    if (other.Lookaheads.AddRangeUnique(set))
                                        other.Dirty = true;
                                }
                                else
                                    state.Closure.Add(conf2);
                            }
                        }
                    }
                }

                i = -1;
                for(int j = 0; j < state.Count; j++)
                {
                    if(state[j].Dirty)
                    {
                        i = j;
                        break;
                    }
                }
            }
        }

        void StepState(RuleState state, Queue<RuleState> queue, FirstSetCache firstCache, Logger logger)
        {
            foreach (var c in state.All)
            {
                if (!c.IsLast)
                {
                    RuleState newState = new RuleState(_States.Count);
                    newState.Header.Add(new RuleConfiguration(c.Rule, c.Pos + 1, c.Lookaheads));

                    var t = c.GetNext();
                    foreach (var c2 in state.All)
                    {
                        if (!c2.IsLast && c2.GetNext().Type == t.Type && c2.GetNext().Name == t.Name)
                        {
                            var n = new RuleConfiguration(c2.Rule, c2.Pos + 1, c2.Lookaheads);
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

        void GenerateActionTable(Environment env, Logger logger)
        {
            _ActionTable.Clear();

            foreach(RuleState state in _States)
            {
                foreach(RuleConfiguration conf in state.All)
                {
                    if(conf.Rule.Group == env.Start &&
                        conf.IsLast &&
                        conf.Lookaheads.Contains((RuleLookahead)null))//Accept
                    {
                        var a = _ActionTable.Get(state.ID, null);
                        if (a != null && a.Action != ActionTable.Action.Accept)
                            Statistics.BU.Conflicts.Add(new BUStatistics.ConflictEntry(BUStatistics.ConflictType.Accept, state));

                        _ActionTable.Set(state.ID, null, ActionTable.Action.Accept, -1);
                    }
                    else if(conf.IsLast)//Reduce
                    {
                        foreach (var l in conf.Lookaheads.Lookaheads)
                        {
                            var a = _ActionTable.Get(state.ID, l);
                            if (a != null)
                            {
                                if (a.Action != ActionTable.Action.Shift && a.StateID != conf.Rule.ID)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ReduceReduce, state, l));
                                else if (a.Action == ActionTable.Action.Shift)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ShiftReduce, state, l));
                            }

                            _ActionTable.Set(state.ID, l, ActionTable.Action.Reduce, conf.Rule.ID);
                        }
                    }
                    else if(conf.GetNext().Type == RuleTokenType.Token)//Shift
                    {
                        RuleToken next = conf.GetNext();
                        RuleLookahead look = new RuleLookahead(next);
                        RuleState found = null;
                        foreach(RuleState.Connection c in state.Production)
                        {
                            if(c.Token == next)
                            {
                                if (found != null)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.Internal, state, look));
                                else
                                    found = c.State;
                            }
                        }

                        var a = _ActionTable.Get(state.ID, look);
                        if (a != null)
                        {
                            if (a.Action != ActionTable.Action.Shift)
                                Statistics.BU.Conflicts.Add(
                                    new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ShiftReduce, state, look));
                            else if (a.Action == ActionTable.Action.Shift && a.StateID != found.ID)
                                Statistics.BU.Conflicts.Add(
                                    new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ShiftShift, state, look));
                        }

                        _ActionTable.Set(state.ID, look, ActionTable.Action.Shift, found.ID);
                    }
                }
            }
        }

        void GenerateGotoTable(Environment env, Logger logger)
        {
            _GotoTable.Clear();

            foreach (RuleState state in _States)
            {
                foreach (var c in state.Production)
                {
                    if(c.Token.Type == RuleTokenType.Rule)
                    {
                        _GotoTable.Set(state.ID, c.Token.Group, c.State.ID);
                    }
                }
            }
        }

        public void Generate(Environment env, Logger logger)
        {
            env.FirstCache.Setup(env, K);
            env.FollowCache.Setup(env, K);

            GenerateStates(env, logger);
            GenerateActionTable(env, logger);
            GenerateGotoTable(env, logger);
        }
    }
}
