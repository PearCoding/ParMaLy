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

namespace PML
{
    [System.Diagnostics.DebuggerDisplay("Count = {_Lookaheads.Count}")]
    public class RuleLookaheadSet : IEquatable<RuleLookaheadSet>, IEnumerable<RuleLookahead>
    {
        List<RuleLookahead> _Lookaheads = new List<RuleLookahead>();

        public List<RuleLookahead> Lookaheads { get { return _Lookaheads; } }

        public RuleLookahead this [int index]
        {
            get { return _Lookaheads[index]; }
        } 

        public RuleLookaheadSet()
        {
        }

        public RuleLookaheadSet(IEnumerable<RuleLookahead> tokens)
        {
            foreach (var s in tokens)
                _Lookaheads.Add(s);
        }

        public RuleLookaheadSet(IEnumerable<RuleToken> tokens)
        {
            foreach(var s in tokens)
                _Lookaheads.Add(new RuleLookahead(s));
        }

        public void Add(RuleLookahead lookahead)
        {
            _Lookaheads.Add(lookahead);
        }

        public void AddUnique(IEnumerable<RuleLookahead> looks)
        {
            foreach(var l in looks)
            {
                if (!_Lookaheads.Contains(l))
                    _Lookaheads.Add(l);
            }
        }

        public void AddUnique(RuleLookaheadSet set)
        {
            AddUnique(set._Lookaheads);
        }

        public void AddUnique(RuleLookahead l)
        {
            if (!_Lookaheads.Contains(l))
                _Lookaheads.Add(l);
        }

        public bool Contains(RuleLookahead lookahead)
        {
            return _Lookaheads.Contains(lookahead);
        }

        public bool Contains(RuleToken str)
        {
            foreach(var l in _Lookaheads)
            {
                if (l[0] == str)
                    return true;
            }

            return false;
        }

        public bool Empty { get { return _Lookaheads.Count == 0; } }

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

            return EnumeratorUtils.ScrambledEquals(_Lookaheads, p._Lookaheads);
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
    }
}
