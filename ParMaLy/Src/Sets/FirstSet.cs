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

namespace PML
{
    public static class FirstSet
    {
        // null is Empty!
        public static void Setup(Environment env)
        {
            // First rule (2)
            foreach (Rule r in env.Rules)
            {
                if (r.Group.FirstSet == null)
                    r.Group.FirstSet = new List<string>();

                if (r.IsEmpty)
                {
                    if (!r.Group.FirstSet.Contains(null))
                        r.Group.FirstSet.Add(null);
                }
                else if (r.Tokens.Count == 1 && r.Tokens[0].Type == RuleTokenType.Token)
                {
                    RuleToken t = r.Tokens[0];
                    if (!r.Group.FirstSet.Contains(t.String))
                        r.Group.FirstSet.Add(t.String);
                }
            }

            // Second rule (3)
            var stack = new Stack<KeyValuePair<Rule, Rule>>();
            foreach (Rule r in env.Rules)
            {
                Rule3(env, r, stack);
            }
        }

        static void Rule3(Environment env, Rule rule, Stack<KeyValuePair<Rule, Rule>> stack)
        {
            if (rule.Group.FirstSet == null)
                rule.Group.FirstSet = new List<string>();

            bool empty = true;
            foreach (RuleToken t in rule.Tokens)
            {
                if (t.Type == RuleTokenType.Token)
                {
                    if (!rule.Group.FirstSet.Contains(t.String))
                        rule.Group.FirstSet.Add(t.String);

                    empty = false;
                    break;
                }
                else
                {
                    RuleGroup grp = env.GroupByName(t.String);

                    foreach (Rule r in grp.Rules)
                    {
                        var pair = new KeyValuePair<Rule, Rule>(rule, r);

                        if (stack.Contains(pair))
                            continue;
                        else
                        {
                            stack.Push(pair);
                            Rule3(env, r, stack);
                        }
                    }
                    
                    foreach (string s in grp.FirstSet)
                    {
                        if (s != null && !rule.Group.FirstSet.Contains(s))
                            rule.Group.FirstSet.Add(s);
                    }

                    if (!grp.FirstSet.Contains(null))
                    {
                        empty = false;
                        break;
                    }
                }
            }

            if (empty)
            {
                if (!rule.Group.FirstSet.Contains(null))
                    rule.Group.FirstSet.Add(null);
            }
        }

        public static List<string> Generate(Environment env, string source)
        {
            return Generate(env, env.ParseLine(source));
        }

        public static List<string> Generate(Environment env, List<RuleToken> tokens)
        {
            List<string> list = new List<string>();

            bool empty = true;
            foreach (RuleToken t in tokens)
            {
                if (t.Type == RuleTokenType.Token)
                {
                    if (!list.Contains(t.String))
                        list.Add(t.String);

                    empty = false;
                    break;
                }
                else
                {
                    RuleGroup grp = env.GroupByName(t.String);

                    foreach (string s in grp.FirstSet)
                    {
                        if (s != null && !list.Contains(s))
                            list.Add(s);
                    }

                    if (!grp.FirstSet.Contains(null))
                    {
                        empty = false;
                        break;
                    }
                }
            }

            if (empty)
            {
                if (!list.Contains(null))
                    list.Add(null);
            }

            return list;
        }
    }
}
