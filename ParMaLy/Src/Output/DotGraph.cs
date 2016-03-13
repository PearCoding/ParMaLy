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
        public static void PrintGroupGraph(TextWriter writer, Environment env, Style.DotStyle style)
        {
            Graph.GroupGraph grpGraph = new Graph.GroupGraph();
            grpGraph.Generate(env);

            writer.WriteLine("digraph G {");
            if(!string.IsNullOrEmpty(style.Graph))
                writer.WriteLine("graph[" + style.Graph + "];");
            if (!string.IsNullOrEmpty(style.Node))
                writer.WriteLine("node[" + style.Node + "];");
            if (!string.IsNullOrEmpty(style.Edge))
                writer.WriteLine("edge[" + style.Edge + "];");
            if (env.Start != null)
                writer.WriteLine("\t" + env.Start.Name + " [" + style.StartNode + "]");

            foreach(RuleGroup grp in env.Groups)
            {
                if(grp != env.Start)
                    writer.WriteLine("\t" + grp.Name + " [" + style.InnerNode + "]");
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

        public static void PrintStateGraph(TextWriter writer, Environment env, RuleState root, Style.DotStyle style)
        {
            writer.WriteLine("digraph G {");
            if (!string.IsNullOrEmpty(style.Graph))
                writer.WriteLine("graph[" + style.Graph + "];");
            if (!string.IsNullOrEmpty(style.Node))
                writer.WriteLine("node[" + style.Node + "];");
            if (!string.IsNullOrEmpty(style.Edge))
                writer.WriteLine("edge[" + style.Edge + "];");
            PrintStateGraphNode(writer, env, root, new Stack<RuleState>(), style);
            writer.WriteLine("}");
            writer.Flush();
        }

        static void PrintStateGraphNode(TextWriter writer, Environment env, RuleState node,
            Stack<RuleState> stack, Style.DotStyle style)
        {
            stack.Push(node);
            string confLabel = "";
            if (style.UseNodeLabel)
            {
                foreach (RuleConfiguration conf in node.All)
                {
                    int p = 0;
                    foreach (var t in conf.Rule.Tokens)
                    {
                        if (p == conf.Pos)
                            confLabel += style.PosIdentificator + " ";

                        if (t.Type == RuleTokenType.Token)
                            confLabel += style.TokenNamePrefix + t.Name + style.TokenNameSuffix + " ";
                        else
                            confLabel += (!style.UseRuleName ?
                                (style.RuleNamePrefix + env.GroupByName(t.Name).ID + style.RuleNameSuffix) : t.Name) + " ";
                        p++;
                    }

                    if (p == conf.Pos)
                        confLabel += style.PosIdentificator + " ";

                    confLabel += style.NodeLabelJustification;
                }

                if (style.MaxNodeLabelLength > 0 && confLabel.Length > style.MaxNodeLabelLength)
                {
                    confLabel = confLabel.Substring(0, style.MaxNodeLabelLength) + "..." + style.NodeLabelJustification;
                }
            }

            string nodestyle = style.InnerNode;
            if (node.Count != 0 && node.First.Rule.Group == env.Start && node.First.Pos == 0)
                nodestyle = style.StartNode;

            writer.WriteLine(node.ID
                + " [" + nodestyle 
                + (!style.UseNodeXLabel ? "" : ",xlabel=\"" + style.NodeXLabelPrefix + (node.ID + style.StateIDOffset) + "\"")
                + (!style.UseNodeLabel ? "" : ",label=\"" + confLabel + "\"") 
                + "];");

            foreach(var c in node.Production)
            {
                string label = "";
                if(!style.UseRuleName && c.Token.Type == RuleTokenType.Rule)
                {
                    label = style.RuleNamePrefix + env.GroupByName(c.Token.Name).ID + style.RuleNameSuffix;
                }
                else if(c.Token.Type == RuleTokenType.Token)
                {
                    label = style.TokenNamePrefix + c.Token.Name + style.TokenNameSuffix;
                }
                else
                {
                    label = c.Token.Name;
                }

                writer.WriteLine(node.ID + " -> " + c.State.ID +
                    (!style.UseEdgeLabel ? "" : " [label=\"" + style.EdgeLabelPrefix + label + "\"") + "];");
            }

            foreach (var c in node.Production)
            {
                if (!stack.Contains(c.State))
                    PrintStateGraphNode(writer, env, c.State, stack, style);
            }
        }
    }
}
