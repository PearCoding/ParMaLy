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

using System;
using System.Linq;
using System.IO;

namespace PML.Output
{
    using Statistics;

    public static class SetFile
    {
        public static void PrintFirstSets(TextWriter writer, FirstSetCache cache, Environment env)
        {            
            for (int i = 1; i <= cache.MaxK; ++i)
            {
                writer.WriteLine("K = " + i);
                foreach (RuleGroup grp in env.Groups)
                {
                    if (cache.Has(grp, i))
                    {
                        writer.WriteLine("  " + grp.Name + ":");
                        var set = cache.Get(grp, i);
                        foreach(var l in set)
                        {
                            if(l == null)
                                writer.WriteLine("    /* EMPTY */");
                            else
                                writer.WriteLine("    " + l.Join(",", v => (v == null ? "$" :
                            (v.Type == RuleTokenType.Token && !v.IsComplex ? "'" + v.Name + "'" : v.Name))));
                        }
                    }
                }

                if(i != cache.MaxK)
                {
                    for (int j = 0; j < 80; ++j)
                        writer.Write("-");
                    writer.WriteLine();
                }
            }

            writer.Flush();
        }
    }
}
