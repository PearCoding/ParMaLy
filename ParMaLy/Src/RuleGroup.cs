using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PML
{
    public class RuleGroup
    {
        int _ID;
        public int ID { get { return _ID; } }

        string _Name;
        public string Name { get { return _Name; } }

        public List<Rule> Rules = new List<Rule>();

        public List<string> FirstSet;
        public List<string> FollowSet;

        public RuleGroup(int id, string name)
        {
            _ID = id;
            _Name = name;
        }
    }
}
