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

namespace PML.Style
{
    public class Style
    {
        public DotStyle GroupDot = new DotStyle();
        public DotStyle StateDot = new DotStyle();

        public HtmlStyle ActionTableHtml = new HtmlStyle();
        public HtmlStyle GotoTableHtml = new HtmlStyle();
        public HtmlStyle TransitionTableHtml = new HtmlStyle();
    }

    public class DotStyle
    {
        public string StartNode =           "style=filled,fillcolor=\"#666666\",fontcolor=white";
        public string InnerNode =           "style=filled,fillcolor=\"#ffe070\"";

        public string Graph =               "splines=true,overlap=false,fontname=\"arial\"";
        public string Node =                "fontname=\"arial\"";
        public string Edge =                "fontname=\"arial\"";

        public string PosIdentificator =    "\u2022";

        public int StateIDOffset =          0;

        public bool UseNodeXLabel =         true;
        public string NodeXLabelPrefix =    "I";

        public bool UseNodeLabel =          true;
        public int MaxNodeLabelLength =     0;
        public string NodeLabelPrefix =     "";
        public string NodeLabelJustification = "\\l";

        public bool UseEdgeLabel = true;
        public string EdgeLabelPrefix = "";

        public bool UseRuleName = true;
        public string RuleNamePrefix = "<";
        public string RuleNameSuffix = ">";
        public string TokenNamePrefix = "'";
        public string TokenNameSuffix = "'";
    }

    public class HtmlStyle
    {
        public string CSS = "style.css";
        public string Table_ID = "pml_table";
        public string Table_Class = "pml_table";

        public string TableAction_Class = "action_table";
        public string TableGoto_Class = "goto_table";
        public string TableTransition_Class = "transition_table";

        public string TableTr_ID_Prefix = "row_";
        public string TableTh_ID_Prefix = "header_";
        public string TableTd_ID_Prefix = "item_";

        public string TableTr_Class = "";
        public string TableTh_Class = "";
        public string TableTd_Class = "";

        public string TableActionShift_Class = "act_shift";
        public string TableActionReduce_Class = "act_reduce";
        public string TableActionAccept_Class = "act_accept";
        public string TableActionEmpty_Class = "act_empty";

        public string TableGotoState_Class = "goto_state";
        public string TableGotoEmpty_Class = "goto_empty";

        public string TableActionShift_Prefix = "s";
        public string TableActionReduce_Prefix = "r";
        public string TableActionAccept_Content = "Accept";
        public string TableActionEmpty_Content = "";

        public string TableGotoState_Prefix = "";
        public string TableGotoEmpty_Content = "";

        public string EOF_Identificator = "$";
        public bool UseRuleName = true;
        public string RuleNamePrefix = "<";
        public string RuleNameSuffix = ">";
        public string TokenNamePrefix = "'";
        public string TokenNameSuffix = "'";
    }
}
