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
using System.Diagnostics;
using System.Linq;

namespace PML.Parser
{
    using R;
    using Statistics;

    class RD : IRParser
    {
        public string Name { get { return "Recursive-Descent"; } }

        Statistics _Statistics;

        public Statistics Statistics { get { return _Statistics; } }

        readonly List<RState> _States = new List<RState>();
        public IEnumerable<RState> States { get { return _States; } }

        readonly int MaxK;

        public int K { get { return MaxK; } }

        public RD(int maxK)
        {
            MaxK = maxK;
        }

        public void Generate(Environment env, Logger logger)
        {
            _States.Clear();
            _Statistics = new Statistics();
            _Statistics.R = new RStatistics();

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            int currentMaxK = 1;
            env.FirstCache.Setup(env, 1);
            env.FollowCache.Setup(env, 1);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach (RuleGroup grp in env.Groups)
            {
                RState state = new RState(grp);

                if (grp.Rules.Count == 1)//k == 0
                {
                    state.Lookaheads.Add(grp.Rules.First(), null);
                }
                else
                {
                    Dictionary<Rule, RuleLookaheadSet> tokens = new Dictionary<Rule, RuleLookaheadSet>();
                    foreach (Rule r in grp.Rules)//TODO: Need a better and responsive approach
                    {
                        RuleLookaheadSet set = null;
                        Dictionary<Rule, RuleLookaheadSet> conflicts = new Dictionary<Rule, RuleLookaheadSet>(tokens);
                        for (int k = 1; k <= MaxK; ++k)
                        {
                            if (k > currentMaxK)
                            {
                                currentMaxK = k;
                                env.FirstCache.Setup(env, k);
                                env.FollowCache.Setup(env, k);
                            }

                            set = PredictSet.Generate(env, grp, r.Tokens, k);

                            Dictionary<Rule, RuleLookaheadSet> newConflict = new Dictionary<Rule, RuleLookaheadSet>();
                            foreach (KeyValuePair<Rule, RuleLookaheadSet> other in conflicts)
                            {
                                foreach (RuleLookahead o in other.Value)
                                {
                                    if (set.HasIntersection(o))
                                    {
                                        RuleLookaheadSet newSet = PredictSet.Generate(env, grp, other.Key.Tokens, k);
                                        newConflict.Add(other.Key, newSet);
                                        tokens[other.Key] = newSet;
                                        break;
                                    }
                                }
                            }

                            conflicts = newConflict;
                        }

                        tokens.Add(r, set);

                        if (conflicts.Count != 0)
                        {
                            _Statistics.R.Conflicts.Add(
                                new RStatistics.ConflictEntry(RStatistics.ConflictType.Decision, state));
                        }
                    }

                    foreach (KeyValuePair<Rule, RuleLookaheadSet> t in tokens)
                    {
                        state.Lookaheads.Add(t.Key, t.Value);
                    }
                }

                _States.Add(state);
            }
            watch.Stop();

            _Statistics.TimeElapsed = watch.ElapsedMilliseconds;
        }
    }
}
