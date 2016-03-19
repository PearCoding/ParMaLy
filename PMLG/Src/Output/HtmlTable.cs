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
using System.Linq;
using System.IO;

namespace PML.Output
{
    public static class HtmlTable
    {
        public static void PrintActionTable(TextWriter writer, BU.ActionTable table, Environment env, Style.HtmlStyle style)
        {
            AddHeader(writer, style);
            writer.WriteLine("<table class='" + style.Table_Class + " " + style.TableAction_Class + "' id='" + style.Table_ID + "'>");

            int tr = 0;
            writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>\n<th></th>");
            tr++;

            var tokens = table.Colums;
            AddActionHeader(0, writer, tokens, style);
            writer.WriteLine("</tr>");

            foreach(var state in table.Rows)
            {
                writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>");
                tr++;

                AddFirstColumn(writer, state, style);
                int td = 1;
                foreach (var s in tokens)
                {
                    var e = table.Get(state, s);
                    AddActionEntry(td, e, writer, style);
                    td++;
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
            AddFooter(writer, style);
            writer.Flush();
        }

        public static void PrintGotoTable(TextWriter writer, BU.GotoTable table, Environment env, Style.HtmlStyle style)
        {
            AddHeader(writer, style);
            writer.WriteLine("<table class='" + style.Table_Class + " " + style.TableGoto_Class + "' id='" + style.Table_ID + "'>");

            int tr = 0;
            writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>\n<th></th>");
            tr++;
            AddGotoHeader(0, writer, env.Groups, style);
            writer.WriteLine("</tr>");

            foreach (var state in table.Rows)
            {
                writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>");
                tr++;

                AddFirstColumn(writer, state, style);
                int td = 1;
                foreach (var s in env.Groups)
                {
                    var e = table.Get(state, s);
                    AddGotoEntry(td, e, writer, style);
                    td++;
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
            AddFooter(writer, style);
            writer.Flush();
        }

        public static void PrintTransitionTable(TextWriter writer, BU.ActionTable actionTable, BU.GotoTable gotoTable, Environment env, Style.HtmlStyle style)
        {
            AddHeader(writer, style);
            writer.WriteLine("<table class='" + style.Table_Class + " " + style.TableTransition_Class + "' id='" + style.Table_ID + "'>");

            int tr = 0;
            writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>\n<th></th>");
            tr++;
            var tokens = actionTable.Colums;
            AddActionHeader(0, writer, tokens, style);
            AddGotoHeader(tokens.Count(), writer, env.Groups, style);
            writer.WriteLine("</tr>");

            foreach (var state in actionTable.Rows)
            {
                writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>");
                tr++;

                AddFirstColumn(writer, state, style);
                int td = 1;
                foreach (var s in tokens)
                {
                    var e = actionTable.Get(state, s);
                    AddActionEntry(td, e, writer, style);
                    td++;
                }

                foreach (var s in env.Groups)
                {
                    var e = gotoTable.Get(state, s);
                    AddGotoEntry(td, e, writer, style);
                    td++;
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
            AddFooter(writer, style);
            writer.Flush();
        }

        public static void PrintLookupTable(TextWriter writer,
            TD.LookupTable lookup, Environment env, Style.HtmlStyle style)
        {
            AddHeader(writer, style);
            writer.WriteLine("<table class='" + style.Table_Class + " " + style.TableLookup_Class + "' id='" + style.Table_ID + "'>");

            int tr = 0;
            writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>\n<th></th>");
            tr++;
            var tokens = lookup.Colums;
            AddActionHeader(0, writer, tokens, style);
            writer.WriteLine("</tr>");

            foreach (var grp in env.Groups)
            {
                writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>");
                tr++;

                AddFirstColumn(writer, grp, style);
                int td = 1;
                foreach (var s in tokens)
                {
                    var e = lookup.Get(grp, s);//For now
                    AddLookupEntry(td, e, writer, style);
                    td++;
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
            AddFooter(writer, style);
            writer.Flush();
        }

        //Parts
        static void AddHeader(TextWriter writer, Style.HtmlStyle style)
        {
            if (!string.IsNullOrEmpty(style.CSS))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine("<link rel=\"stylesheet\" href=\"" + style.CSS + "\" />");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
            }
        }

        static void AddFooter(TextWriter writer, Style.HtmlStyle style)
        {
            if (!string.IsNullOrEmpty(style.CSS))
            {
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        static void AddFirstColumn(TextWriter writer, int state, Style.HtmlStyle style)
        {
            writer.WriteLine("<td class='" + style.TableTd_Class + "' id='" + style.TableTd_ID_Prefix + "0'>"
                    + state + "</td>");
        }

        static void AddFirstColumn(TextWriter writer, RuleGroup group, Style.HtmlStyle style)
        {
            writer.WriteLine("<td class='" + style.TableTd_Class + "' id='" + style.TableTd_ID_Prefix + "0'>"
                    + group.Name + "</td>");
        }

        static void AddActionHeader(int off, TextWriter writer, IEnumerable<RuleLookahead> tokens, Style.HtmlStyle style)
        {
            int th = off;
            foreach (var s in tokens)
            {
                writer.WriteLine("<th class='" + style.TableTh_Class + "' id='" + style.TableTh_ID_Prefix + th + "'>"
                    + (s == null ? style.EOF_Identificator : s.ToString()) + "</th>");//TODO: Remove prefix etc.
                th++;
            }
        }

        static void AddActionEntry(int td, BU.ActionTable.Entry e, TextWriter writer, Style.HtmlStyle style)
        {
            string special = "";
            if (e != null)
            {
                if (e.Action == BU.ActionTable.Action.Accept)
                    special = style.TableActionAccept_Class;
                else if (e.Action == BU.ActionTable.Action.Reduce)
                    special = style.TableActionReduce_Class;
                else if (e.Action == BU.ActionTable.Action.Shift)
                    special = style.TableActionShift_Class;
                else
                    special = style.TableActionEmpty_Class;
            }

            writer.Write("<td class='" + style.TableTd_Class + " " + special + "' id='" + style.TableTd_ID_Prefix + td + "'>");

            if (e != null)
            {
                if (e.Action == BU.ActionTable.Action.Accept)
                    writer.Write(style.TableActionAccept_Content);
                else if (e.Action == BU.ActionTable.Action.Reduce)
                    writer.Write(style.TableActionReduce_Prefix + e.StateID);
                else if (e.Action == BU.ActionTable.Action.Shift)
                    writer.Write(style.TableActionShift_Prefix + e.StateID);
                else if (!string.IsNullOrEmpty(style.TableActionEmpty_Content))
                    writer.Write(style.TableActionEmpty_Content);
            }

            writer.WriteLine("</td>");
        }

        static void AddGotoHeader(int off, TextWriter writer, List<RuleGroup> grps, Style.HtmlStyle style)
        {
            int th = off;
            foreach (var s in grps)
            {
                writer.WriteLine("<th class='" + style.TableTh_Class + "' id='" + style.TableTh_ID_Prefix + th + "'>"
                    + (style.UseRuleName ? s.Name : style.RuleNamePrefix + s.ID + style.RuleNameSuffix) + "</th>");
                th++;
            }
        }

        static void AddGotoEntry(int td, BU.GotoTable.Entry e, TextWriter writer, Style.HtmlStyle style)
        {
            string special = "";
            if (e != null)
                special = style.TableGotoState_Class;
            else
                special = style.TableGotoEmpty_Class;


            writer.Write("<td class='" + style.TableTd_Class + " " + special + "' id='" + style.TableTd_ID_Prefix + td + "'>");

            if (e != null)
                writer.Write(style.TableGotoState_Prefix + e.StateID);
            else if (!string.IsNullOrEmpty(style.TableGotoEmpty_Content))
                writer.Write(style.TableGotoEmpty_Content);

            writer.WriteLine("</td>");
        }

        static void AddLookupEntry(int td, TD.LookupTable.Entry e, TextWriter writer, Style.HtmlStyle style)
        {
            string special = "";
            if (e != null)
                special = style.TableLookupRule_Class;
            else
                special = style.TableLookupEmpty_Class;


            writer.Write("<td class='" + style.TableTd_Class + " " + special + "' id='" + style.TableTd_ID_Prefix + td + "'>");

            if (e != null)
                writer.Write(style.TableLookupRule_Prefix + e.Rule.ID);
            else if (!string.IsNullOrEmpty(style.TableLookupEmpty_Content))
                writer.Write(style.TableLookupEmpty_Content);

            writer.WriteLine("</td>");
        }
    }
}
