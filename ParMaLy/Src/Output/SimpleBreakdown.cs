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
using System.Linq;
using System.IO;

namespace PML.Output
{
    public static class SimpleBreakdown
    {
        public static void Print(TextWriter writer, Environment env, Parser.IBTParser parser)
        {
            writer.WriteLine("Tokens: " + String.Join(", ", env.Tokens.Select(s => "'" + s + "'").ToArray()));
            writer.WriteLine("Groups: " + String.Join(", ", env.Groups.Select(s => s.Name).ToArray()));
            writer.WriteLine("Start: " + (env.Start == null ? "NOT SET!" : env.Start.Name));

            writer.WriteLine("Rules:");
            foreach (Rule r in env.Rules)
            {
                writer.Write("  [" + r.ID + "] " + r.Group.Name + ": ");
                if (r.IsEmpty)
                    writer.Write("/*EMPTY*/");
                else
                {
                    foreach (RuleToken t in r.Tokens)
                    {
                        if (t.Type == RuleTokenType.Rule)
                            writer.Write("<" + t.Name + "> ");
                        else
                            writer.Write("'" + t.Name + "' ");
                    }
                }
                writer.WriteLine();
            }

            Graph.GroupGraph grpGraph = new Graph.GroupGraph();
            grpGraph.Generate(env);
            writer.WriteLine("Group Graph:");
            foreach(var node in grpGraph.Nodes)
            {
                writer.WriteLine("  [" + node.Group.Name + "]:");
                foreach (var con in node.Connections)
                {
                    writer.WriteLine("    --> [" + con.Group.Name + "]");
                }
            }

            writer.WriteLine("First Sets:");
            foreach (RuleGroup grp in env.Groups)
            {
                writer.WriteLine("  " + grp.Name + ": { " 
                    + String.Join(", ", grp.FirstSet.Select(s => (s == null ? "/*EMPTY*/" : "'" + s + "'")).ToArray())
                    + " }");
            }

            writer.WriteLine("Follow Sets:");
            foreach (RuleGroup grp in env.Groups)
            {
                writer.WriteLine("  " + grp.Name + ": { "
                    + String.Join(", ", grp.FollowSet.Select(s => (s == null ? "/*EOF*/" : "'" + s + "'")).ToArray())
                    + " }");
            }

            if (parser != null)
            {
                writer.WriteLine("Statistics:");
                var statistics = parser.Statistics;
                writer.WriteLine("  Elapsed: " + statistics.TimeElapsed + "ms");

                var process = statistics.Proceedings.Aggregate((l, r) => l.TimeElapsed > r.TimeElapsed ? l : r);
                if(process != null)
                    writer.WriteLine("  Longest process: State " + process.Job.ID + " with " + process.TimeElapsed + "ms");

                writer.WriteLine("  Conflict count: " + statistics.Conflicts.Count);

                if (statistics.Conflicts.Count != 0)
                {
                    writer.WriteLine("  Conflicts:");
                    foreach (var e in statistics.Conflicts)
                    {
                        string special = "";
                        string token = "";
                        switch (e.Type)
                        {
                            case Statistics.BTStatistics.ConflictType.ShiftReduce:
                                special = "SRC";
                                token = " " + e.Token != null ? e.Token : "$";
                                break;
                            case Statistics.BTStatistics.ConflictType.ReduceReduce:
                                special = "RRC";
                                token = " " + e.Token != null ? e.Token : "$";
                                break;
                            case Statistics.BTStatistics.ConflictType.ShiftShift:
                                special = "SSC";
                                token = " " + e.Token != null ? e.Token : "$";
                                break;
                            case Statistics.BTStatistics.ConflictType.Accept:
                                special = "AC";
                                break;
                            case Statistics.BTStatistics.ConflictType.Internal:
                                special = "Int";
                                break;
                        }

                        writer.WriteLine("    [" + special + "] State (" + e.State.ID + ")" + token);
                    }
                }
            }

            writer.Flush();
        }
    }
}
