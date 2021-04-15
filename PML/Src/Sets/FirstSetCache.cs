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
using System.Linq;

namespace PML
{
    public class FirstSetCache
    {
        readonly Dictionary<KeyValuePair<Rule, int>, RuleLookaheadSet> _RuleCache =
            new Dictionary<KeyValuePair<Rule, int>, RuleLookaheadSet>();
        readonly Dictionary<KeyValuePair<IEnumerable<RuleToken>, int>, RuleLookaheadSet> _ExprCache =
            new Dictionary<KeyValuePair<IEnumerable<RuleToken>, int>, RuleLookaheadSet>();
        readonly List<int> _Setups = new List<int>();

        public int MaxK { get { return _Setups.Max(); } }

        public void Setup(Environment env, int k)
        {
            if (!_Setups.Contains(k))
            {
                _Setups.Add(k);
                FirstSet.Setup(env, this, k);
            }
        }

        public RuleLookaheadSet Generate(IEnumerable<RuleToken> tokens, int k)
        {
            KeyValuePair<IEnumerable<RuleToken>, int> key = new KeyValuePair<IEnumerable<RuleToken>, int>(tokens, k);
            if (_ExprCache.ContainsKey(key))
            {
                return _ExprCache[key];
            }
            else
            {
                RuleLookaheadSet res = FirstSet.Generate(tokens, k, this);

                if (k >= 1)
                    _ExprCache.Add(key, res);

                return res;
            }
        }

        public bool Has(Rule r, int k)
        {
            return _RuleCache.ContainsKey(new KeyValuePair<Rule, int>(r, k));
        }

        public bool Has(RuleGroup grp, int k)
        {
            foreach (KeyValuePair<KeyValuePair<Rule, int>, RuleLookaheadSet> p in _RuleCache)
            {
                if (p.Key.Key.Group == grp && p.Key.Value == k)
                    return true;
            }
            return false;
        }

        public void Set(Rule r, int k, RuleLookaheadSet set)
        {
            _RuleCache[new KeyValuePair<Rule, int>(r, k)] = set;
        }

        public RuleLookaheadSet Get(Rule r, int k)
        {
            return _RuleCache[new KeyValuePair<Rule, int>(r, k)];
        }

        public RuleLookaheadSet Get(RuleGroup grp, int k)
        {
            RuleLookaheadSet set = new RuleLookaheadSet();
            foreach (Rule r in grp.Rules)
            {
                if (Has(r, k))
                {
                    RuleLookaheadSet s = Get(r, k);
                    set.AddRangeUnique(s);
                }
            }
            return set;
        }

        public void Clear()
        {
            _RuleCache.Clear();
            _ExprCache.Clear();
            _Setups.Clear();
        }
    }
}
