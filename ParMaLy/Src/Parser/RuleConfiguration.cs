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

namespace PML.Parser
{
    public class RuleConfiguration
    {
        Rule _Rule;
        public Rule Rule { get { return _Rule; } }

        int _Pos;
        public int Pos { get { return _Pos; } }

        public RuleConfiguration(Rule rule, int pos)
        {
            _Rule = rule;
            _Pos = pos;
        }

        public bool IsFirst { get { return Pos == 0; } }

        public bool IsLast { get { return Rule.Tokens.Count == Pos; } }

        public RuleToken GetNext()
        {
            if (Rule.Tokens.Count == Pos)
                return null;

            return Rule.Tokens[Pos];
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            RuleConfiguration p = obj as RuleConfiguration;
            return Equals(p);
        }

        public bool Equals(RuleConfiguration p)
        {
            if ((object)p == null)
                return false;
            
            return (Rule == p.Rule) && (Pos == p.Pos);
        }
        public override int GetHashCode()
        {
            return _Rule.GetHashCode() ^ _Pos;
        }

        public static bool operator == (RuleConfiguration a, RuleConfiguration b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;
            
            if ((object)a == null)
                return false;
            
            return a.Equals(b);
        }

        public static bool operator != (RuleConfiguration a, RuleConfiguration b)
        {
            return !(a == b);
        }
    }
}
