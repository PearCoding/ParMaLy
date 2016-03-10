using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML.Style
{
    class Style
    {
        public DotStyle GroupDot = new DotStyle();
        public DotStyle StateDot = new DotStyle();
    }

    class DotStyle
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
}
