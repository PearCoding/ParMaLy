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

namespace PML
{
    public enum RuleTokenType
    {
        Token,
        Rule,
    }

    public class RuleToken : System.IEquatable<RuleToken>
    {
        Rule _Parent;
        public Rule Parent { get { return _Parent; } }

        RuleTokenType _Type;
        public RuleTokenType Type { get { return _Type; } }

        string _String;
        public string Name { get { return _String; } }

        public RuleGroup Group;

        public RuleToken(Rule parent, RuleTokenType type, string str)
        {
            _Parent = parent;
            _Type = type;
            _String = str;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            RuleToken p = obj as RuleToken;
            return Equals(p);
        }

        public bool Equals(RuleToken other)
        {
            if ((object)other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _Parent == other._Parent && _Type == other._Type && _String == other._String;
        }

        public override int GetHashCode()
        {
            return _Parent.GetHashCode() ^ _Type.GetHashCode() ^ _String.GetHashCode();
        }

        public static bool operator ==(RuleToken a, RuleToken b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if ((object)a == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(RuleToken a, RuleToken b)
        {
            return !(a == b);
        }
    }
}
