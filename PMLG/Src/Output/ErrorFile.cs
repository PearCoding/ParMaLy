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
using System.IO;
using System.Linq;

namespace PML.Output
{
    using Statistics;

    static class ErrorFile
    {
        public static void Print(TextWriter writer, Statistics stats)
        {
            if (stats.BU != null)
            {
                PrintBU(writer, stats.BU);
            }
            else if (stats.TD != null)
            {
                PrintTD(writer, stats.TD);
            }
            else if (stats.R != null)
            {
                PrintR(writer, stats.R);
            }

            writer.Flush();
        }

        static void PrintBU(TextWriter writer, BUStatistics stats)
        {
            writer.WriteLine("Conflict count: " + stats.Conflicts.Count);

            if (stats.Conflicts.Count != 0)
            {
                writer.WriteLine("Conflicts:");
                foreach (BUStatistics.ConflictEntry e in stats.Conflicts)
                {
                    string special = "";
                    string token = "";
                    switch (e.Type)
                    {
                        case BUStatistics.ConflictType.ShiftReduce:
                            special = "SRC";
                            token = " with lookahead " + (e.Lookahead != null ? e.Lookahead.ToString() : "$");
                            break;
                        case BUStatistics.ConflictType.ReduceReduce:
                            special = "RRC";
                            token = " with lookahead " + (e.Lookahead != null ? e.Lookahead.ToString() : "$");
                            break;
                        case BUStatistics.ConflictType.ShiftShift:
                            special = "SSC";
                            token = " with lookahead " + (e.Lookahead != null ? e.Lookahead.ToString() : "$");
                            break;
                        case BUStatistics.ConflictType.Accept:
                            special = "AC";
                            break;
                        case BUStatistics.ConflictType.Internal:
                            special = "Int";
                            break;
                    }

                    writer.WriteLine("  [" + special + "] State (" + e.State.ID + ")" + token);
                }
            }
        }

        static void PrintTD(TextWriter writer, TDStatistics stats)
        {
            writer.WriteLine("Conflict count: " + stats.Conflicts.Count);

            if (stats.Conflicts.Count != 0)
            {
                writer.WriteLine("Conflicts:");
                foreach (TDStatistics.ConflictEntry e in stats.Conflicts)
                {
                    string special = "";
                    string token = "";
                    switch (e.Type)
                    {
                        case TDStatistics.ConflictType.Lookup:
                            special = "LC";
                            token = " with lookahead " + (e.Lookahead != null ? e.Lookahead.ToString() : "$");
                            break;
                        case TDStatistics.ConflictType.Internal:
                            special = "Int";
                            break;
                    }

                    writer.WriteLine("  [" + special + "] Rule (" + e.Rule.ID + ") in " + e.Group.Name + token);
                }
            }
        }

        static void PrintR(TextWriter writer, RStatistics stats)
        {
            writer.WriteLine("Conflict count: " + stats.Conflicts.Count);

            if (stats.Conflicts.Count != 0)
            {
                writer.WriteLine("Conflicts:");
                foreach (RStatistics.ConflictEntry e in stats.Conflicts)
                {
                    string special = "";
                    switch (e.Type)
                    {
                        case RStatistics.ConflictType.Decision:
                            special = "Decision";
                            break;
                        case RStatistics.ConflictType.Internal:
                            special = "Internal";
                            break;
                    }

                    writer.WriteLine("  [" + special + "] State " + e.State.Group.Name
                        + " with lookahead " + String.Join("; ",
                            e.State.Lookaheads.Select(v => v.Value.ToString()).ToArray()));
                }
            }
        }
    }
}
