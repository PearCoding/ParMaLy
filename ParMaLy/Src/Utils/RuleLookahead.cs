using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML
{
    public class RuleLookahead
    {
        string[] _Tokens;

        public string this [int index]
        {
            get { return _Tokens[index]; }
            set { _Tokens[index] = value; }
        } 

        public RuleLookahead(int count)
        {
            _Tokens = new string[count];
        }

        public RuleLookahead(string[] tokens)
        {
            _Tokens = tokens;
        }

        public RuleLookahead(string token)
        {
            _Tokens = new string[] { token };
        }

        public string Join(string delim)
        {
            return String.Join(delim, _Tokens);
        }
    }
}
