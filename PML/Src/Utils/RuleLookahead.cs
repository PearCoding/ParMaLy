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

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace PML
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RuleLookahead : IEquatable<RuleLookahead>, IEnumerable<RuleToken>
    {
        List<RuleToken> _Tokens;

        public int Count { get { return _Tokens.Count; } }

        public RuleToken this [int index]
        {
            get { return _Tokens[index]; }
            set { _Tokens[index] = value; }
        }
        public RuleLookahead()
        {
            _Tokens = new List<RuleToken>();
        }

        public RuleLookahead(IEnumerable<RuleToken> tokens)
        {
            _Tokens = tokens.ToList();
        }

        public RuleLookahead(RuleToken token)
        {
            _Tokens = new List<RuleToken>();
            _Tokens.Add(token);
        }

        public void Add(RuleToken token)
        {
            _Tokens.Add(token);
        }

        public void AddRange(IEnumerable<RuleToken> tokens)
        {
            _Tokens.AddRange(tokens);
        }

        public string Join(string delim)
        {
            return String.Join(delim, _Tokens.Select(v => v.Name).ToArray());
        }

        public string Join(string delim, Func<RuleToken, string> selector) 
        {
            return String.Join(delim, _Tokens.Select(selector).ToArray());
        }

        public override string ToString()
        {
            return "[" + Join(",", v => (v == null ? "$" : "'" + v.Name + "'")) + "]";
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

            if (_Tokens.Count != p._Tokens.Count)
                return false;

            return _Tokens.SequenceEqual(p._Tokens);
        }

        public override int GetHashCode()
        {
            return EnumeratorUtils.GetOrderDependentHashCode(_Tokens);
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

        //IEnumerable
        public IEnumerator<RuleToken> GetEnumerator()
        {
            return ((IEnumerable<RuleToken>)_Tokens).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<RuleToken>)_Tokens).GetEnumerator();
        }

        //System
        private string DebuggerDisplay { get { return ToString(); } }
    }
}
