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
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace PML.Output
{
    static class PMLWriter
    {
        public static void WriteLL(TextWriter writer, Parser.ITDParser parser, Environment env)
        {
            writer.WriteLine("PML LL " + parser.K);
            WriteLookaheads(writer, parser.Lookup.Colums);

            foreach (var grp in parser.Lookup.Rows)
            {
                writer.Write(grp.ID + " ");
                foreach(var column in parser.Lookup.Colums)
                {
                    var entry = parser.Lookup.Get(grp, column);
                    if(entry != null && entry.Rule != null)
                    {
                        writer.Write("" + entry.Rule.ID);
                    }
                    else
                    {
                        writer.Write("-");
                    }

                    writer.Write(" ");
                }
                writer.WriteLine();
            }
            writer.Flush();
        }

        static void WriteLookaheads(TextWriter writer, IEnumerable<RuleLookahead> lookaheads)
        {
            writer.WriteLine(String.Join(" ", lookaheads.Select(v => v == null ? "-" : "[" +
                v.Join(",", t => t.ID.ToString()) + "]").ToArray()));
        }
    }
}
