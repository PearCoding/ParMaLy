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

using System.Linq;

namespace PML.Style
{
    using DataLisp;
    public static class StyleParser
    {
        public static Style Parse(string source, PML.Logger logger)
        {
            DataLisp lisp = new DataLisp(logger);
            DataContainer container = new DataContainer();

            lisp.Parse(source);
            lisp.Build(container);

            Style style = new Style();

            if(container.TopGroups.Count >= 1)
            {
                DataGroup first = container.TopGroups.First();

                if(first.ID == "pml_style")
                {
                    Data group_dot = first.FromKey("group_dot");
                    Data state_dot = first.FromKey("state_dot");

                    if(group_dot != null && group_dot.Type == DataType.Group)
                    {
                        if(group_dot.Group.ID == "dot_style")
                            style.GroupDot = ParseDotStyle(group_dot.Group);
                    }

                    if (state_dot != null && state_dot.Type == DataType.Group)
                    {
                        if (state_dot.Group.ID == "dot_style")
                            style.StateDot = ParseDotStyle(state_dot.Group);
                    }
                }
            }
            return style;
        }

        static DotStyle ParseDotStyle(DataGroup grp)
        {
            DotStyle style = new DotStyle();

            Data start_node = grp.FromKey("start_node");
            Data inner_node = grp.FromKey("inner_node");
            Data graph = grp.FromKey("graph");
            Data node = grp.FromKey("node");
            Data edge = grp.FromKey("edge");
            Data pos_identificator = grp.FromKey("pos_identificator");
            Data state_id_offset = grp.FromKey("state_id_offset");
            Data use_node_xlabel = grp.FromKey("use_node_xlabel");
            Data node_xlabel_prefix = grp.FromKey("node_xlabel_prefix");
            Data use_node_label = grp.FromKey("use_node_label");
            Data max_node_label_length = grp.FromKey("max_node_label_length");
            Data node_label_prefix = grp.FromKey("node_label_prefix");
            Data node_label_justification = grp.FromKey("node_label_justification");
            Data use_edge_label = grp.FromKey("use_edge_label");
            Data edge_label_prefix = grp.FromKey("edge_label_prefix");
            Data use_rule_name = grp.FromKey("use_rule_name");
            Data rule_name_prefix = grp.FromKey("rule_name_prefix");
            Data rule_name_suffix = grp.FromKey("rule_name_suffix");
            Data token_name_prefix = grp.FromKey("token_name_prefix");
            Data token_name_suffix = grp.FromKey("token_name_suffix");

            if (start_node != null && start_node.Type == DataType.String)
                style.StartNode = start_node.String;

            if (inner_node != null && inner_node.Type == DataType.String)
                style.InnerNode = inner_node.String;

            if (graph != null && graph.Type == DataType.String)
                style.Graph = graph.String;

            if (node != null && node.Type == DataType.String)
                style.Node = node.String;

            if (edge != null && edge.Type == DataType.String)
                style.Edge = edge.String;

            if (pos_identificator != null && pos_identificator.Type == DataType.String)
                style.PosIdentificator = pos_identificator.String;

            if (state_id_offset != null && state_id_offset.Type == DataType.Integer)
                style.StateIDOffset = state_id_offset.Integer;

            if (use_node_xlabel != null && use_node_xlabel.Type == DataType.Bool)
                style.UseNodeXLabel = use_node_xlabel.Bool;

            if (node_xlabel_prefix != null && node_xlabel_prefix.Type == DataType.String)
                style.NodeXLabelPrefix = node_xlabel_prefix.String;

            if (use_node_label != null && use_node_label.Type == DataType.Bool)
                style.UseNodeLabel = use_node_label.Bool;

            if (max_node_label_length != null && max_node_label_length.Type == DataType.Integer)
                style.MaxNodeLabelLength = max_node_label_length.Integer;

            if (node_label_prefix != null && node_label_prefix.Type == DataType.String)
                style.NodeLabelPrefix = node_label_prefix.String;

            if (node_label_justification != null && node_label_justification.Type == DataType.String)
                style.NodeLabelJustification = node_label_justification.String;

            if (use_edge_label != null && use_edge_label.Type == DataType.Bool)
                style.UseEdgeLabel = use_edge_label.Bool;

            if (edge_label_prefix != null && edge_label_prefix.Type == DataType.String)
                style.EdgeLabelPrefix = edge_label_prefix.String;

            if (use_rule_name != null && use_rule_name.Type == DataType.Bool)
                style.UseRuleName = use_rule_name.Bool;

            if (rule_name_prefix != null && rule_name_prefix.Type == DataType.String)
                style.RuleNamePrefix = rule_name_prefix.String;

            if (rule_name_suffix != null && rule_name_suffix.Type == DataType.String)
                style.RuleNameSuffix = rule_name_suffix.String;

            if (token_name_prefix != null && token_name_prefix.Type == DataType.String)
                style.TokenNamePrefix = token_name_prefix.String;

            if (token_name_suffix != null && token_name_suffix.Type == DataType.String)
                style.TokenNameSuffix = token_name_suffix.String;

            return style;
        }
    }
}
