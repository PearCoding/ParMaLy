﻿/*
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
    using BU;
    using Statistics;

    public class SLR : IBUParser
    {
        readonly IBUParser _BUP = null;
        readonly ActionTable _ActionTable = new ActionTable();
        Statistics _Statistics;
        public string Name { get { return "SLR(" + K + ", " + D + ")"; } }

        readonly int _K = 1;
        public int K { get { return _K; } }

        readonly int _D = 0;
        public int D { get { return _D; } }

        public List<RuleState> States { get { return _BUP.States; } }

        public RuleState StartState { get { return _BUP.StartState; } }

        public ActionTable ActionTable { get { return _ActionTable; } }

        public GotoTable GotoTable { get { return _BUP.GotoTable; } }

        public Statistics Statistics { get { return _Statistics; } }

        public SLR(int k = 1, int d = 0)
        {
            _K = 1;

            if (d <= 0)
                _BUP = new Parser.LR0();
            else
                _BUP = new Parser.LR(d);
        }

        void GenerateActionTable(Environment env, Logger logger)
        {
            _ActionTable.Clear();

            foreach (RuleState state in States)
            {
                foreach (RuleConfiguration conf in state.All)
                {
                    if (conf.Rule.Group == env.Start && conf.IsLast)//Accept
                    {
                        ActionTable.Entry a = _ActionTable.Get(state.ID, null);
                        if (a != null && a.Action != ActionTable.Action.Accept)
                            Statistics.BU.Conflicts.Add(new BUStatistics.ConflictEntry(BUStatistics.ConflictType.Accept, state));

                        _ActionTable.Set(state.ID, null, ActionTable.Action.Accept, -1);
                    }
                    else if (conf.IsLast)
                    {
                        RuleLookaheadSet follow = env.FollowCache.Get(conf.Rule.Group, K);
                        foreach (RuleLookahead look in follow)
                        {
                            ActionTable.Entry a = _ActionTable.Get(state.ID, look);
                            if (a != null)
                            {
                                if (a.Action != ActionTable.Action.Shift && a.StateID != conf.Rule.ID)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ReduceReduce, state, look));
                                else if (a.Action == ActionTable.Action.Shift)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.ShiftReduce, state, look));
                            }

                            _ActionTable.Set(state.ID, look, ActionTable.Action.Reduce, conf.Rule.ID);
                        }
                    }
                    else if (conf.GetNext().Type == RuleTokenType.Token)
                    {
                        RuleToken next = conf.GetNext();
                        RuleLookahead look = new RuleLookahead(next);
                        RuleState found = null;
                        foreach (RuleState.Connection c in state.Production)
                        {
                            if (c.Token == next)
                            {
                                if (found != null)
                                    Statistics.BU.Conflicts.Add(
                                        new BUStatistics.ConflictEntry(BUStatistics.ConflictType.Internal, state, look));
                                else
                                    found = c.State;
                            }
                        }

                        ActionTable.Entry a = _ActionTable.Get(state.ID, look);
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

        public void Generate(Environment env, Logger logger)
        {
            env.FirstCache.Setup(env, K);
            env.FollowCache.Setup(env, K);

            _BUP.Generate(env, logger);
            _Statistics = _BUP.Statistics;
            _Statistics.BU.Conflicts.Clear();

            GenerateActionTable(env, logger);
        }
    }
}
