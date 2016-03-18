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

using System;
using System.Diagnostics;

namespace PML.Parser
{
    using Statistics;
    using TD;

    public class LLK : ITDParser
    {
        LookupTable _Lookup = new LookupTable();

        public string Name { get { return "LL(" + K + ")"; } }

        public LookupTable Lookup { get { return _Lookup; } }

        Statistics _Statistics;
        public Statistics Statistics { get { return _Statistics; } }

        int _K;

        public int K { get { return _K; } }

        public LLK(int k)
        {
            _K = k;
        }

        public void Generate(Environment env, Logger logger)
        {
            GenerateTable(env, logger);
        }

        public void GenerateTable(Environment env, Logger logger)
        {
            _Lookup.Clear();
            _Statistics = new Statistics();
            _Statistics.TD = new TDStatistics();

            env.FirstCache.Setup(env, K);
            env.FollowCache.Setup(env, K);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach(var grp in env.Groups)
            {
                foreach (var r in grp.Rules)
                {
                    var predict = PredictSet.Generate(env, grp, r.Tokens, K);
                    foreach (var s in predict)
                    {
                        if (_Lookup.Get(grp, s) != null)
                        {
                            _Statistics.TD.Conflicts.Add(
                                new TDStatistics.ConflictEntry(TDStatistics.ConflictType.Lookup, grp, s, r));
                        }

                        _Lookup.Set(grp, s, r);   
                    }
                }
            }
            watch.Stop();
            _Statistics.TimeElapsed = watch.ElapsedMilliseconds;
        }
    }
}
