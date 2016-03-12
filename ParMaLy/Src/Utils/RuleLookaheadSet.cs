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
    public class RuleLookaheadSet : IEquatable<RuleLookaheadSet>
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

        public RuleLookaheadSet(string[] tokens)
        {
            foreach(var s in tokens)
                _Lookaheads.Add(new RuleLookahead(tokens));
        }

        public void Add(RuleLookahead lookahead)
        {
            _Lookaheads.Add(lookahead);
        }

        public bool Contains(RuleLookahead lookahead)
        {
            return _Lookaheads.Contains(lookahead);
        }

        public bool Contains(string str)
        {
            foreach(var l in _Lookaheads)
            {
                if (l[0] == str)
                    return true;
            }

            return false;
        }

        public bool Empty { get { return _Lookaheads.Count == 0; } }

        public bool Equals(RuleLookaheadSet p)
        {
            if ((object)p == null)
                return false;

            if (ReferenceEquals(this, p))
                return true;

            if (_Lookaheads.Count != p._Lookaheads.Count)
                return false;

            return RuleState.ScrambledEquals(_Lookaheads, p._Lookaheads);
        }
    }
}
