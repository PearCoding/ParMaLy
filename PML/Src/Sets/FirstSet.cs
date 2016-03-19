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
    public static class FirstSet
    {
        // Should work with Caches
        public static RuleLookaheadSet Generate(IEnumerable<RuleToken> tokens, int k, FirstSetCache cache)
        {
            if (k < 1 || tokens.Count() == 0)
            {
                var s = new RuleLookaheadSet();
                s.Add((RuleLookahead)null);
                return s;
            }
            else
            {
                RuleLookaheadSet set = new RuleLookaheadSet();
                set.Add((RuleLookahead)null);

                bool hasTerminals = true;
                foreach(var t in tokens)
                {
                    if(t.Type == RuleTokenType.Rule)
                    {
                        hasTerminals = false;
                        break;
                    }
                }

                int maxLength = (hasTerminals && tokens.Count() < k ?
                    tokens.Count() : k);//TODO: Really??

                for (int i = 0; i < tokens.Count(); ++i)
                {
                    var t = tokens.ElementAt(i);
                    if (t.Type == RuleTokenType.Token)
                    {
                        var preSet = new RuleLookaheadSet();
                        foreach (var l in set)
                        {
                            if (l != null && l.Count == maxLength)
                                preSet.Add(l);
                            else
                            {
                                var look = new RuleLookahead();
                                if (l != null)
                                    look.Add(l);

                                look.Add(t);
                                preSet.AddUnique(look);
                            }
                        }
                        set = preSet;
                    }
                    else
                    {
                        var otherSet = cache.Get(t.Group, k);
                        var newSet = new RuleLookaheadSet();
                        foreach (var l in set)
                        {
                            if (l != null && l.Count == k)
                                newSet.Add(l);
                            else
                            {
                                foreach (var o in otherSet)
                                {
                                    if (o == null)
                                        newSet.AddUnique(l);
                                    else
                                    {
                                        var look = new RuleLookahead();

                                        if (l != null)
                                            look.Add(l);

                                        if (o.Count < (k - look.Count))
                                            look.Add(o);
                                        else
                                            look.Add(o.Take(k - look.Count));

                                        newSet.AddUnique(look);
                                    }
                                }
                            }
                        }
                        set = newSet;
                    }
                    
                    bool finished = true;
                    foreach (var s in set)
                    {
                        if (s == null || s.Count < k)
                        {
                            finished = false;
                            break;
                        }
                    }

                    if (finished)
                        break;
                }

                return set;
            }
        }

        public static void Setup(Environment env, FirstSetCache cache, int k)
        {
            // Setup caches
            foreach (var r in env.Rules)
            {
                cache.Set(r, k, new RuleLookaheadSet());
            }
            
            Stack<KeyValuePair<Rule, Rule>> stack = new Stack<KeyValuePair<Rule, Rule>>();
            foreach (var r in env.Rules)
            {
                SetupInternal(cache, r, stack, k);
            }
        }

        static void SetupInternal(FirstSetCache cache, Rule rule, Stack<KeyValuePair<Rule, Rule>> stack, int k)
        {
            RuleLookaheadSet set = cache.Get(rule, k);
            if (rule.IsEmpty)
            {
                set.AddUnique((RuleLookahead)null);
            }
            else
            {
                if (set.Count() == 0)
                {
                    set.Add(null);
                }

                int maxLength = (!rule.HasNonTerminals && rule.Tokens.Count() < k ?
                    rule.Tokens.Count() : k);//TODO: Really??

                for (int i = 0; i < rule.Tokens.Count(); ++i)
                {
                    var t = rule.Tokens.ElementAt(i);

                    if (t.Type == RuleTokenType.Token)
                    {
                        var preSet = new RuleLookaheadSet();
                        foreach (var l in set)
                        {
                            if (l != null && l.Count == maxLength)
                                preSet.Add(l);
                            else
                            {
                                var look = new RuleLookahead();
                                if (l != null)
                                    look.Add(l);

                                look.Add(t);
                                preSet.AddUnique(look);
                            }
                        }
                        set = preSet;
                    }
                    else if (t.Type == RuleTokenType.Rule)
                    {
                        foreach (Rule r in t.Group.Rules)
                        {
                            var pair = new KeyValuePair<Rule, Rule>(rule, r);

                            if (stack.Contains(pair))
                                continue;
                            else
                            {
                                stack.Push(pair);
                                SetupInternal(cache, r, stack, k);
                            }
                        }
                        var otherSet = cache.Get(t.Group, k);

                        var newSet = new RuleLookaheadSet();
                        foreach (var l in set)
                        {
                            if (l != null && l.Count == maxLength)
                                newSet.Add(l);
                            else
                            {
                                foreach (var o in otherSet)
                                {
                                    if (o == null)
                                        newSet.AddUnique(l);
                                    else
                                    {
                                        var look = new RuleLookahead();
                                        if (l != null)
                                            look.Add(l);

                                        if (o.Count <= (maxLength - look.Count))
                                            look.Add(o);
                                        else
                                            look.Add(o.Take(maxLength - look.Count));

                                        newSet.AddUnique(look);
                                    }
                                }
                            }
                        }
                        set = newSet;
                    }

                    cache.Set(rule, k, set);

                    bool finished = true;
                    foreach (var s in set)
                    {
                        if (s == null || (s.Count < maxLength))
                        {
                            finished = false;
                            break;
                        }
                    }

                    if (finished)
                        break;
                }
            }
        }
    }
}
