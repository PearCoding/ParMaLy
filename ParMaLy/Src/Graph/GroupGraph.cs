using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PML.Graph
{
    public class GroupGraph
    {
        List<GroupNode> _Nodes = new List<GroupNode>();
        public List<GroupNode> Nodes { get { return _Nodes; } }

        public GroupNode NodeByGroup(RuleGroup grp)
        {
            foreach(GroupNode node in _Nodes)
            {
                if (grp == node.Group)
                    return node;
            }
            return null;
        }

        public void Generate(Environment env)
        {
            _Nodes.Clear();

            foreach (RuleGroup grp in env.Groups)
            {
                _Nodes.Add(new GroupNode(grp));
            }

            foreach (GroupNode node in _Nodes)
            {
                foreach(Rule r in node.Group.Rules)
                {
                    foreach(RuleToken t in r.Tokens)
                    {
                        if(t.Type == RuleTokenType.Rule)
                        {
                            GroupNode other = NodeByGroup(env.GroupByName(t.String));

                            if (!node.Connections.Contains(other))
                                node.Connections.Add(other);
                        }
                    }
                }
            }
        }
    }
}
