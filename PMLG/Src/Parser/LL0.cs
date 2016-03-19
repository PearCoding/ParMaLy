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
using System.Collections.Generic;

namespace PML.Parser
{
    using PML.Statistics;
    using TD;

    //Pretty useless type of parser... but it's a type.
    public class LL0 : ITDParser
    {
        LookupTable _Lookup = new LookupTable();

        public string Name { get { return "LL(0)"; } }

        public int K { get { return 0; } }

        public LookupTable Lookup { get { return _Lookup; } }

        Statistics _Statistics;
        public Statistics Statistics { get { return _Statistics; } }

        public void Generate(Environment env, Logger logger)
        {
            GenerateTable(env, logger);
        }

        /*
         The LL(0) parser does not use these tables, but due to the framework design,
         we do generate a useless one.
         A LL(0) would only need the current Non-Terminal to make decisions. 
         */
        void GenerateTable(Environment env, Logger logger)
        {
            _Lookup.Clear();
            _Statistics = new Statistics();
            _Statistics.TD = new TDStatistics();

            var tokens = new List<RuleToken>(env.Tokens);
            tokens.Add(null);//EOF

            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach(var grp in env.Groups)
            {
                foreach (var r in grp.Rules)
                {
                    foreach (var s in tokens)
                    {
                        var look = (s == null ? null : new RuleLookahead(s));
                        if (_Lookup.Get(grp, look) != null)
                        {
                            _Statistics.TD.Conflicts.Add(
                                new TDStatistics.ConflictEntry(TDStatistics.ConflictType.Lookup, grp, look, r));
                        }

                        _Lookup.Set(grp, look, r);
                    }
                }
            }
            watch.Stop();
            _Statistics.TimeElapsed = watch.ElapsedMilliseconds;
        }
    }
}
