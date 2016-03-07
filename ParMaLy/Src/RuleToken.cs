using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PML
{
    public enum RuleTokenType
    {
        Token,
        Rule,
    }

    public class RuleToken
    {
        Rule _Parent;
        public Rule Parent { get { return _Parent; } }

        RuleTokenType _Type;
        public RuleTokenType Type { get { return _Type; } }

        string _String;
        public string String { get { return _String; } }

        public RuleToken(Rule parent, RuleTokenType type, string str)
        {
            _Parent = parent;
            _Type = type;
            _String = str;
        }
    }
}
