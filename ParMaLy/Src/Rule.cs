using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PML
{
    public class Rule
    {
        int _ID;
        public int ID { get { return _ID; } }

        RuleGroup _Group;
        public RuleGroup Group { get { return _Group; } }

        public List<RuleToken> Tokens = new List<RuleToken>();

        public Rule(int id, RuleGroup grp)
        {
            _ID = id;
            _Group = grp;
        }

        public bool IsEmpty
        {
            get
            {
                return Tokens.Count == 0;
            }
        }
    }
}
