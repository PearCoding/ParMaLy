using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML
{
    public class RuleLookahead : IEquatable<RuleLookahead>
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

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            RuleLookahead p = obj as RuleLookahead;
            return Equals(p);
        }

        public bool Equals(RuleLookahead p)
        {
            if ((object)p == null)
                return false;

            if (ReferenceEquals(this, p))
                return true;

            if (_Tokens.Length != p._Tokens.Length)
                return false;

            for(int i = 0; i < _Tokens.Length; ++i)
            {
                if (_Tokens[i] != p._Tokens[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (_Tokens.GetHashCode() ^ 56).GetHashCode();
        }

        public static bool operator == (RuleLookahead a, RuleLookahead b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if ((object)a == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator != (RuleLookahead a, RuleLookahead b)
        {
            return !(a == b);
        }
    }
}
