using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PML.Output
{
    public static class SimpleBreakdown
    {
        public static void Print(TextWriter writer, Environment env)
        {
            writer.WriteLine("Simple Breakdown>>");
            writer.WriteLine("Tokens: " + String.Join(", ", env.Tokens.Select(s => "'" + s + "'")));
            writer.WriteLine("Groups: " + String.Join(", ", env.Groups.Select(s => s.Name)));
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
                            writer.Write("<" + t.String + "> ");
                        else
                            writer.Write("'" + t.String + "' ");
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
            FirstSet.Setup(env);
            foreach (RuleGroup grp in env.Groups)
            {
                writer.WriteLine("  " + grp.Name + ": { " 
                    + String.Join(", ", grp.FirstSet.Select(s => (s == null ? "/*EMPTY*/" : "'" + s + "'"))) 
                    + " }");
            }

            writer.Flush();
        }
    }
}
