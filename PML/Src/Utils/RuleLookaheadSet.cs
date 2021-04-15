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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PML
{
    [System.Diagnostics.DebuggerDisplay("Set {DebuggerDisplay,nq}")]
    public class RuleLookaheadSet : IEquatable<RuleLookaheadSet>, IEnumerable<RuleLookahead>
    {
        readonly List<RuleLookahead> _Lookaheads = new List<RuleLookahead>();

        public List<RuleLookahead> Lookaheads { get { return _Lookaheads; } }

        public RuleLookahead this[int index]
        {
            get { return _Lookaheads[index]; }
        }

        public RuleLookaheadSet()
        {
        }

        public RuleLookaheadSet(IEnumerable<RuleLookahead> tokens)
        {
            foreach (RuleLookahead s in tokens)
                _Lookaheads.Add(s);
        }

        public void Add(RuleLookahead lookahead)
        {
            _Lookaheads.Add(lookahead);
        }

        public void AddRange(IEnumerable<RuleLookahead> looks)
        {
            _Lookaheads.AddRange(looks);
        }

        public bool AddUnique(RuleLookahead l)
        {
            if (!_Lookaheads.Contains(l))
            {
                _Lookaheads.Add(l);
                return true;
            }
            else
                return false;
        }

        public bool AddRangeUnique(IEnumerable<RuleLookahead> looks)
        {
            bool added = false;
            foreach (RuleLookahead l in looks)
            {
                if (!_Lookaheads.Contains(l))
                {
                    added = true;
                    _Lookaheads.Add(l);
                }
            }
            return added;
        }

        public bool Contains(RuleLookahead lookahead)
        {
            return _Lookaheads.Contains(lookahead);
        }

        public bool HasIntersection(RuleLookahead lookahead)
        {
            foreach (RuleLookahead look in _Lookaheads)
            {
                if ((look == null && lookahead == null) ||
                    (look != null && lookahead != null && lookahead.HasIntersection(look)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Empty { get { return _Lookaheads.Count == 0; } }

        public override string ToString()
        {
            return "[" + String.Join(",", _Lookaheads.Select(v => v != null ? v.ToString() : "$").ToArray()) + "]";
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            RuleLookaheadSet p = obj as RuleLookaheadSet;
            return Equals(p);
        }

        public bool Equals(RuleLookaheadSet p)
        {
            if ((object)p == null)
                return false;

            if (ReferenceEquals(this, p))
                return true;

            if (_Lookaheads.Count != p._Lookaheads.Count)
                return false;

            if (_Lookaheads.Contains(null) != p._Lookaheads.Contains(null))
                return false;

            return EnumeratorUtils.ScrambledEquals(_Lookaheads.Where(v => v != null), p._Lookaheads.Where(v => v != null));
        }

        public override int GetHashCode()
        {
            return EnumeratorUtils.GetOrderIndependentHashCode(_Lookaheads);
        }

        public static bool operator ==(RuleLookaheadSet a, RuleLookaheadSet b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if ((object)a == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(RuleLookaheadSet a, RuleLookaheadSet b)
        {
            return !(a == b);
        }

        //IEnumerable
        public IEnumerator<RuleLookahead> GetEnumerator()
        {
            return ((IEnumerable<RuleLookahead>)_Lookaheads).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<RuleLookahead>)_Lookaheads).GetEnumerator();
        }

        //System
        private string DebuggerDisplay { get { return ToString(); } }
    }
}
