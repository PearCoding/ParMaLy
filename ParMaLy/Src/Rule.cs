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

        string _Name;
        public string Name { get { return _Name; } }

        public List<RuleToken> Tokens = new List<RuleToken>();

        public Rule(int id, string name)
        {
            _ID = id;
            _Name = name;
        }
    }
}
