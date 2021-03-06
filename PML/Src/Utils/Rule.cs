﻿/*
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
using System.Collections.Generic;
using System.Linq;

namespace PML
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Rule : IEquatable<Rule>
    {
        readonly int _ID;
        public int ID { get { return _ID; } }

        readonly RuleGroup _Group;
        public RuleGroup Group { get { return _Group; } }

        readonly string _Code;
        public string Code { get { return _Code; } }

        public List<RuleToken> Tokens = new List<RuleToken>();

        public Rule(int id, RuleGroup grp, string code)
        {
            _ID = id;
            _Group = grp;
            _Code = code;
        }

        public bool IsEmpty
        {
            get
            {
                return Tokens.Count == 0;
            }
        }

        public bool HasNonTerminals
        {
            get
            {
                foreach (RuleToken t in Tokens)
                {
                    if (t.Type == RuleTokenType.Rule)
                        return true;
                }
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Rule p = obj as Rule;
            return Equals(p);
        }

        public bool Equals(Rule other)
        {
            if ((object)other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _ID == other._ID;//We assume ID is unique.
        }

        public override int GetHashCode()
        {
            return _ID.GetHashCode();
        }

        public static bool operator ==(Rule a, Rule b)
        {
            if (object.ReferenceEquals(a, b))
                return true;

            if ((object)a == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Rule a, Rule b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return "[" + ID + "] " + _Group.Name + " \u2192 " +
                    (IsEmpty ? "/* EMPTY */" : String.Join(" ",
                        Tokens.Select(v => (v.Type == RuleTokenType.Token ? "'" + v.Name + "'" : v.Name)).ToArray()));
        }
        //System
        private string DebuggerDisplay { get { return ToString(); } }
    }
}
