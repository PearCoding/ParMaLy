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
    public class GotoTable
    {
        public class Entry
        {
            public RuleState State;
            public RuleGroup Group;
        }

        Dictionary<RuleState, List<Entry>> _Table = new Dictionary<RuleState, List<Entry>>();

        public void Clear()
        {
            _Table.Clear();
        }

        public void Set(RuleState state, RuleGroup grp, RuleState n)
        {
            if (!_Table.ContainsKey(state))
                _Table[state] = new List<Entry>();

            var l = _Table[state];
            Entry entry = null;
            foreach(var e in l)
            {
                if (e.Group == grp)
                {
                    entry = e;
                    break;
                }
            }

            if(entry == null)
            {
                entry = new Entry();
                entry.Group = grp;

                l.Add(entry);
            }

            entry.State = n;
        }

        public Entry Get(RuleState state, RuleGroup grp)
        {
            if (!_Table.ContainsKey(state))
                return null;

            var l = _Table[state];
            foreach (var e in l)
            {
                if (e.Group == grp)
                {
                    return e;
                }
            }

            return null;
        }

        public List<RuleState> Rows { get { return new List<RuleState>(_Table.Keys); } }
    }
}
