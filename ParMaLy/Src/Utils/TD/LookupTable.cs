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

using System.Collections.Generic;

namespace PML.TD
{
    //Used by the LL implementation
    public class LookupTable
    {
        public class Entry
        {
            public Rule Rule;
            public RuleLookahead Lookahead;
        }

        Dictionary<RuleGroup, List<Entry>> _Table = new Dictionary<RuleGroup, List<Entry>>();

        public void Clear()
        {
            _Table.Clear();
        }

        public void Set(RuleGroup grp, RuleLookahead lookahead, Rule rule)
        {
            if (!_Table.ContainsKey(grp))
                _Table[grp] = new List<Entry>();

            var l = _Table[grp];
            Entry entry = null;
            foreach(var e in l)
            {
                if (e.Lookahead == lookahead)
                {
                    entry = e;
                    break;
                }
            }

            if(entry == null)
            {
                entry = new Entry();
                entry.Lookahead = lookahead;

                l.Add(entry);
            }

            entry.Rule = rule;
        }

        public Entry Get(RuleGroup grp, RuleLookahead lookahead)
        {
            if (!_Table.ContainsKey(grp))
                return null;

            var l = _Table[grp];
            foreach (var e in l)
            {
                if (e.Lookahead == lookahead)
                    return e;
            }

            return null;
        }

        public IEnumerable<RuleGroup> Rows { get { return _Table.Keys; } }
        public IEnumerable<RuleLookahead> Colums//TODO: We can cache it!
        {
            get
            {
                List<RuleLookahead> columns = new List<RuleLookahead>();
                foreach (var p in _Table)
                {
                    foreach (var e in p.Value)
                    {
                        if (!columns.Contains(e.Lookahead))
                            columns.Add(e.Lookahead);
                    }
                }

                return columns;
            }
        }
    }
}
