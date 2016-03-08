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
    class DotGraph
    {
        public static void PrintGroupGraph(TextWriter writer, Environment env)
        {
            Graph.GroupGraph grpGraph = new Graph.GroupGraph();
            grpGraph.Generate(env);

            writer.WriteLine("digraph G {");
            if (env.Start != null)
                writer.WriteLine("\t" + env.Start.Name + " [style=filled,fillcolor=\"#666666\",fontcolor=white]");

            foreach(RuleGroup grp in env.Groups)
            {
                if(grp != env.Start)
                    writer.WriteLine("\t" + grp.Name + " [style=filled,fillcolor=\"#ffe070\"]");
            }
            foreach (var node in grpGraph.Nodes)
            {
                foreach (var con in node.Connections)
                {
                    writer.WriteLine("\t" + node.Group.Name + " -> " + con.Group.Name);
                }
            }
            writer.WriteLine("}");
            writer.Flush();
        }
        public enum DetailLabelStyle
        {
            None,
            Auto,
            Full
        }

        public static void PrintStateGraph(TextWriter writer, Environment env, RuleState root, DetailLabelStyle dlstyle, bool idOnly)
        {
            writer.WriteLine("digraph G {");
            writer.WriteLine("graph[splines=true,overlap=false,fontname=\"arial\"];");
            writer.WriteLine("node[fontname=\"arial\"];");
            writer.WriteLine("edge[fontname=\"arial\"];");
            PrintStateGraphNode(writer, env, root, new Stack<RuleState>(), dlstyle, idOnly);
            writer.WriteLine("}");
            writer.Flush();
        }

        static void PrintStateGraphNode(TextWriter writer, Environment env, RuleState node,
            Stack<RuleState> stack, DetailLabelStyle dlstyle, bool idOnly)
        {
            stack.Push(node);
            string confLabel = "";
            if (dlstyle != DetailLabelStyle.None)
            {
                foreach (RuleConfiguration conf in node.Configurations)
                {
                    int p = 0;
                    foreach (var t in conf.Rule.Tokens)
                    {
                        if (p == conf.Pos)
                            confLabel += "\u2022 ";

                        if (t.Type == RuleTokenType.Token)
                            confLabel += "'" + t.Name + "' ";
                        else
                            confLabel += (idOnly ? ("<" + env.GroupByName(t.Name).ID + ">") : t.Name) + " ";
                        p++;
                    }

                    if (p == conf.Pos)
                        confLabel += "\u2022 ";

                    confLabel += "\\l";
                }

                if (dlstyle == DetailLabelStyle.Auto && confLabel.Length > 100)
                {
                    confLabel = confLabel.Substring(0, 100) + "...\\l";
                }
            }

            string color = "\"#fff090\"";
            if (node.First.Rule.Group == env.Start)
                color = "\"#cc6030\"";

            writer.WriteLine(node.ID
                + " [shape=box,style=\"rounded,filled\",fillcolor=" + color + ",xlabel=\"I" + node.ID + "\","
                + (dlstyle == DetailLabelStyle.None ? "" : "label=\"" + confLabel + "\"") + "];");
            foreach(var c in node.Production)
            {
                string label = "";
                if(idOnly && c.Token.Type == RuleTokenType.Rule)
                {
                    label = "<" + env.GroupByName(c.Token.Name).ID + ">";
                }
                else if(c.Token.Type == RuleTokenType.Token)
                {
                    label = "'" + c.Token.Name + "'";
                }
                else
                {
                    label = c.Token.Name;
                }

                writer.WriteLine(node.ID + " -> " + c.State.ID + " [label=\"" + label + "\"];");
            }

            foreach (var c in node.Production)
            {
                if (!stack.Contains(c.State))
                    PrintStateGraphNode(writer, env, c.State, stack, dlstyle, idOnly);
            }
        }
    }
}
