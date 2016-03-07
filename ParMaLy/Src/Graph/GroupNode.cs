using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PML.Graph
{
    public class GroupNode
    {
        RuleGroup _Group;
        public RuleGroup Group { get { return _Group; } }

        List<GroupNode> _Connections = new List<GroupNode>();
        public List<GroupNode> Connections { get { return _Connections; } }

        public GroupNode(RuleGroup grp)
        {
            _Group = grp;
        }
    }
}
