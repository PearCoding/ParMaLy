using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace PML.Output
{
    public static class HtmlTable
    {
        public static void PrintActionTable(TextWriter writer, List<RuleState> rows, ActionTable table, Environment env, Style.HtmlStyle style)
        {
            AddHeader(writer, style);
            writer.WriteLine("<table class='" + style.Table_Class + " " + style.TableAction_Class + "' id='" + style.Table_ID + "'>");

            int tr = 0;
            writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>\n<th></th>");
            tr++;

            var tokens = new List<string>(env.Tokens);
            tokens.Add(null);//Add EOF
            AddActionHeader(0, writer, tokens, style);
            writer.WriteLine("</tr>");

            foreach(var state in rows)
            {
                writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>");
                tr++;

                AddFirstColumn(writer, state, style);
                int td = 1;
                foreach (string s in tokens)
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

        public static void PrintGotoTable(TextWriter writer, List<RuleState> rows, GotoTable table, Environment env, Style.HtmlStyle style)
        {
            AddHeader(writer, style);
            writer.WriteLine("<table class='" + style.Table_Class + " " + style.TableGoto_Class + "' id='" + style.Table_ID + "'>");

            int tr = 0;
            writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>\n<th></th>");
            tr++;
            AddGotoHeader(0, writer, env.Groups, style);
            writer.WriteLine("</tr>");

            foreach (var state in rows)
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


        public static void PrintTransitionTable(TextWriter writer, List<RuleState> rows,
            ActionTable actionTable, GotoTable gotoTable, Environment env, Style.HtmlStyle style)
        {
            AddHeader(writer, style);
            writer.WriteLine("<table class='" + style.Table_Class + " " + style.TableTransition_Class + "' id='" + style.Table_ID + "'>");

            int tr = 0;
            writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>\n<th></th>");
            tr++;
            var tokens = new List<string>(env.Tokens);
            tokens.Add(null);//Add EOF
            AddActionHeader(0, writer, tokens, style);
            AddGotoHeader(tokens.Count, writer, env.Groups, style);
            writer.WriteLine("</tr>");

            foreach (var state in rows)
            {
                writer.WriteLine("<tr class='" + style.TableTr_Class + "' id='" + style.TableTr_ID_Prefix + tr + "'>");
                tr++;

                AddFirstColumn(writer, state, style);
                int td = 1;
                foreach (string s in tokens)
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

        static void AddFirstColumn(TextWriter writer, RuleState state, Style.HtmlStyle style)
        {
            writer.WriteLine("<td class='" + style.TableTd_Class + "' id='" + style.TableTd_ID_Prefix + "0'>"
                    + state.ID + "</td>");
        }

        static void AddActionHeader(int off, TextWriter writer, List<string> tokens, Style.HtmlStyle style)
        {
            int th = off;
            foreach (string s in tokens)
            {
                writer.WriteLine("<th class='" + style.TableTh_Class + "' id='" + style.TableTh_ID_Prefix + th + "'>"
                    + (s == null ? style.EOF_Identificator : style.TokenNamePrefix + s + style.TokenNameSuffix) + "</th>");
                th++;
            }
        }

        static void AddActionEntry(int td, ActionTable.Entry e, TextWriter writer, Style.HtmlStyle style)
        {
            string special = "";
            if (e != null)
            {
                if (e.Action == ActionTable.Action.Accept)
                    special = style.TableActionAccept_Class;
                else if (e.Action == ActionTable.Action.Reduce)
                    special = style.TableActionReduce_Class;
                else if (e.Action == ActionTable.Action.Shift)
                    special = style.TableActionShift_Class;
                else
                    special = style.TableActionEmpty_Class;
            }

            writer.Write("<td class='" + style.TableTd_Class + " " + special + "' id='" + style.TableTd_ID_Prefix + td + "'>");

            if (e != null)
            {
                if (e.Action == ActionTable.Action.Accept)
                    writer.Write(style.TableActionAccept_Content);
                else if (e.Action == ActionTable.Action.Reduce && e.State != null)
                    writer.Write(style.TableActionReduce_Prefix + e.State.ID);
                else if (e.Action == ActionTable.Action.Shift && e.State != null)
                    writer.Write(style.TableActionShift_Prefix + e.State.ID);
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

        static void AddGotoEntry(int td, GotoTable.Entry e, TextWriter writer, Style.HtmlStyle style)
        {
            string special = "";
            if (e != null)
                special = style.TableGotoState_Class;
            else
                special = style.TableGotoEmpty_Class;


            writer.Write("<td class='" + style.TableTd_Class + " " + special + "' id='" + style.TableTd_ID_Prefix + td + "'>");

            if (e != null)
                writer.Write(style.TableGotoState_Prefix + e.State.ID);
            else if (!string.IsNullOrEmpty(style.TableGotoEmpty_Content))
                writer.Write(style.TableGotoEmpty_Content);

            writer.WriteLine("</td>");
        }
    }
}
