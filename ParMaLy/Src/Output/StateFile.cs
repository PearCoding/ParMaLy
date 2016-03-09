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

using System.IO;
using System.Collections.Generic;

namespace PML.Output
{
    public static class StateFile
    {
        public static void PrintStates(TextWriter writer, List<RuleState> states, Environment env, bool idOnly = false)
        {
            writer.WriteLine("LR(0) States:");
            foreach(RuleState state in states)
            {
                writer.WriteLine("[" + state.ID + "]");
                foreach(RuleConfiguration conf in state.Configurations)
                {
                    if (idOnly)
                        writer.Write("  " + conf.Rule.Group.ID + "|" + conf.Rule.ID + " -> ");
                    else
                        writer.Write("  " + conf.Rule.Group.Name + " -> ");

                    if (conf.Rule.IsEmpty)
                    {
                        if (idOnly)
                            writer.Write("#");
                        else
                            writer.Write("# /*EMPTY*/");
                    }
                    else
                    {
                        int p = 0;
                        foreach (RuleToken t in conf.Rule.Tokens)
                        {
                            if (p == conf.Pos)
                                writer.Write("# ");

                            if (t.Type == RuleTokenType.Rule)
                            {
                                if(idOnly)
                                {
                                    RuleGroup g = env.GroupByName(t.Name);
                                    writer.Write("<" + g.ID + "> ");
                                }
                                else
                                    writer.Write("<" + t.Name + "> ");
                            }
                            else
                                writer.Write("'" + t.Name + "' ");

                            p++;
                        }

                        if(conf.IsLast)
                            writer.Write("# ");

                        if (conf.Lookahead != null)
                            writer.Write(conf.Lookahead.Join("/"));
                    }
                    writer.WriteLine();
                }
            }

            writer.Flush();
        }
    }
}
