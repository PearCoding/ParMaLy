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
using System.Collections.Generic;

namespace PML
{
    [System.Diagnostics.DebuggerDisplay("<{ID}> {Name,nq}")]
    public class RuleGroup : IEquatable<RuleGroup>
    {
        int _ID;
        public int ID { get { return _ID; } set { _ID = value; } }

        readonly string _Name;
        public string Name { get { return _Name; } }

        readonly string _ReturnType;
        public string ReturnType { get { return _ReturnType; } }

        public List<Rule> Rules = new List<Rule>();

        public RuleGroup(int id, string name, string returnType)
        {
            _ID = id;
            _Name = name;
            _ReturnType = returnType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            RuleGroup p = obj as RuleGroup;
            return Equals(p);
        }

        public bool Equals(RuleGroup other)
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

        public static bool operator ==(RuleGroup a, RuleGroup b)
        {
            if (object.ReferenceEquals(a, b))
                return true;

            if ((object)a == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(RuleGroup a, RuleGroup b)
        {
            return !(a == b);
        }
    }
}
