using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace PML.Output
{
    public static class HtmlTable
    {
        public static void PrintActionTable(TextWriter writer, List<RuleState> rows, ActionTable table, Environment env)
        {
            string style = "border:1px solid black;border-collapse:collapse;";

            writer.WriteLine("<table style='width:100%;"+ style + "'>");

            writer.WriteLine("<tr style='" + style + "'>\n<th></th>");
            var tokens = new List<string>(env.Tokens);
            tokens.Add(null);//Add EOF

            foreach (string s in tokens)
            {
                writer.WriteLine("<th style='" + style + "'>"
                    + (s == null ? "$" : "'" + s + "'") + "</th>");
            }
            writer.WriteLine("</tr>");

            foreach(var state in rows)
            {
                writer.WriteLine("<tr style='" + style + "'>\n"
                    + "<td style='" + style + "'>"
                    + state.ID + "</td>");
                
                foreach (string s in tokens)
                {
                    string special = "";
                    var e = table.Get(state, s);
                    if (e != null)
                    {
                        if (e.Action == ActionTable.Action.Accept)
                            special = "background-color:#66FF66;";
                        else if (e.Action == ActionTable.Action.Reduce)
                            special = "background-color:#FF6633;";
                        else if (e.Action == ActionTable.Action.Shift)
                            special = "background-color:#FFFF66;";
                    }

                    writer.Write("<td align=\"center\" style='" + style + special +"'>");

                    if (e != null)
                    {
                        if (e.Action == ActionTable.Action.Accept)
                            writer.Write("Accept");
                        else if (e.Action == ActionTable.Action.Reduce)
                            writer.Write("r" + e.State.ID);
                        else if (e.Action == ActionTable.Action.Shift)
                            writer.Write("s" + e.State.ID);
                    }

                    writer.WriteLine("</td>");
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
            writer.Flush();
        }

        public static void PrintGotoTable(TextWriter writer, List<RuleState> rows, GotoTable table, Environment env, bool idOnly)
        {
            string style = "border:1px solid black;border-collapse:collapse;";

            writer.WriteLine("<table style='width:100%;" + style + "'>");

            writer.WriteLine("<tr style='" + style + "'>\n<th></th>");
            foreach (var s in env.Groups)
            {
                writer.WriteLine("<th style='" + style + "'>"
                    + (idOnly ? "<" + s.ID + ">" : s.Name) + "</th>");
            }
            writer.WriteLine("</tr>");

            foreach (var state in rows)
            {
                writer.WriteLine("<tr style='" + style + "'>\n"
                    + "<td style='" + style + "'>"
                    + state.ID + "</td>");

                foreach (var s in env.Groups)
                {
                    string special = "";
                    var e = table.Get(state, s);
                    if (e != null)
                    {
                        special = "background-color:#ccc;";
                    }

                    writer.Write("<td style='" + style + special + "'>");

                    if (e != null)
                    {
                        writer.Write("" + e.State.ID);
                    }

                    writer.WriteLine("</td>");
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
            writer.Flush();
        }


        public static void PrintTransitionTable(TextWriter writer, List<RuleState> rows,
            ActionTable actionTable, GotoTable gotoTable, Environment env, bool idOnly)
        {
            string style = "border:1px solid black;border-collapse:collapse;";

            writer.WriteLine("<table style='width:100%;" + style + "'>");

            writer.WriteLine("<tr style='" + style + "'>\n<th></th>");
            var tokens = new List<string>(env.Tokens);
            tokens.Add(null);//Add EOF
            foreach (string s in tokens)
            {
                writer.WriteLine("<th style='" + style + "'>"
                    + (s == null ? "$" : "'" + s + "'") + "</th>");
            }
            foreach (var s in env.Groups)
            {
                writer.WriteLine("<th style='" + style + "'>"
                    + (idOnly ? "<" + s.ID + ">" : s.Name) + "</th>");
            }
            writer.WriteLine("</tr>");

            foreach (var state in rows)
            {
                writer.WriteLine("<tr style='" + style + "'>\n"
                    + "<td style='" + style + "'>"
                    + state.ID + "</td>");

                foreach (string s in tokens)
                {
                    string special = "";
                    var e = actionTable.Get(state, s);
                    if (e != null)
                    {
                        if (e.Action == ActionTable.Action.Accept)
                            special = "background-color:#66FF66;";
                        else if (e.Action == ActionTable.Action.Reduce)
                            special = "background-color:#FF6633;";
                        else if (e.Action == ActionTable.Action.Shift)
                            special = "background-color:#FFFF66;";
                    }

                    writer.Write("<td align=\"center\" style='" + style + special + "'>");

                    if (e != null)
                    {
                        if (e.Action == ActionTable.Action.Accept)
                            writer.Write("Accept");
                        else if (e.Action == ActionTable.Action.Reduce)
                            writer.Write("r" + e.State.ID);
                        else if (e.Action == ActionTable.Action.Shift)
                            writer.Write("s" + e.State.ID);
                    }

                    writer.WriteLine("</td>");
                }

                foreach (var s in env.Groups)
                {
                    string special = "";
                    var e = gotoTable.Get(state, s);
                    if (e != null)
                    {
                        special = "background-color:#ccc;";
                    }

                    writer.Write("<td align=\"center\" style='" + style + special + "'>");

                    if (e != null)
                    {
                        writer.Write("" + e.State.ID);
                    }

                    writer.WriteLine("</td>");
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
            writer.Flush();
        }
    }
}
