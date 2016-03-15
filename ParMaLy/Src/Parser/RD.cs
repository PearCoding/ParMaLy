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
    using R;

    class RD : IRParser
    {
        public string Name { get { return "Recursive-Descent"; } }

        Statistics _Statistics;

        public Statistics Statistics { get { return _Statistics; } }

        List<RState> _States = new List<RState>();
        public IEnumerable<RState> States { get { return _States; } }

        public void Generate(Environment env, Logger logger)//Currently only a LL(1) parser...
        {
            _States.Clear();
            _Statistics = new Statistics();

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            int max = 3;
            int currentMaxK = 1;
            env.FirstCache.Setup(env, 1);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach(var grp in env.Groups)
            {
                RState state = new RState(grp);

                if (grp.Rules.Count == 1)//k == 0
                {
                    state.Lookaheads.Add(grp.Rules.First(), null);
                }
                else
                {
                    List<RuleLookaheadSet> tokens = new List<RuleLookaheadSet>();

                    foreach (var r in grp.Rules)
                    {
                        bool found = false;
                        RuleLookaheadSet l = null;
                        for (int k = 1; k <= max; ++k)
                        {
                            if(k > currentMaxK)
                            {
                                currentMaxK = k;
                                env.FirstCache.Setup(env, k);
                            }

                            l = PredictSet.Generate(env, grp, r.Tokens, k);

                            found = false;
                            foreach (var other in tokens)
                            {
                                foreach (var o in other)
                                {
                                    if (l.Contains(o))
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (found)
                                    break;
                            }

                            if (!found)
                            {
                                tokens.Add(l);
                                RuleLookaheadSet set = new RuleLookaheadSet(l);
                                state.Lookaheads.Add(r, set);
                                break;
                            }
                        }

                        if(found)
                        {
                            logger.Log(LogLevel.Warning, "Coulnd't solve conflict after " + max + " lookaheads");
                            RuleLookaheadSet set = new RuleLookaheadSet(l);
                            state.Lookaheads.Add(r, set);
                        }
                    }
                }

                _States.Add(state);
            }
            watch.Stop();

            _Statistics.TimeElapsed = watch.ElapsedMilliseconds;
        }
    }
}
