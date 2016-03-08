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
using System.Linq;

namespace PML
{
    public class RuleState
    {
        int _ID;
        public int ID { get { return _ID; } }

        List<RuleConfiguration> _Configurations = new List<RuleConfiguration>();
        public List<RuleConfiguration> Configurations { get { return _Configurations; } }

        public class Connection
        {
            public RuleState State;
            public RuleToken Token;
        }

        List<Connection> _Production = new List<Connection>();
        public List<Connection> Production { get { return _Production; } }

        public RuleState(int id)
        {
            _ID = id;
        }

        public RuleConfiguration First { get { return _Configurations.First(); } }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            RuleState p = obj as RuleState;
            return Equals(p);
        }

        public bool Equals(RuleState p)
        {
            if ((object)p == null)
                return false;

            return ScrambledEquals(_Configurations, p.Configurations);
        }
        public override int GetHashCode()
        {
            return _Configurations.GetHashCode() ^ 42;
        }

        public static bool operator == (RuleState a, RuleState b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if ((object)a == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator != (RuleState a, RuleState b)
        {
            return !(a == b);
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}
