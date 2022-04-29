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

namespace PML.Preprocess
{
    static class LeftRecursion
    {
        // Detection
        public static bool HasLeftRecursion(RuleGroup grp, FirstSetCache firstset)
        {
            return HasLeftRecursionFor(grp, grp, firstset, new Stack<RuleGroup>());
        }

        static bool HasLeftRecursionFor(RuleGroup current, RuleGroup search, FirstSetCache firstset, Stack<RuleGroup> stack)
        {
            stack.Push(current);

            foreach (Rule rule in current.Rules)
            {
                foreach (RuleToken t in rule.Tokens)
                {
                    if (t.Type == RuleTokenType.Token)
                        break;
                    else
                    {
                        if (t.Group == search)
                            return true;
                        else
                        {
                            if (!stack.Contains(t.Group))
                            {
                                if (HasLeftRecursionFor(t.Group, search, firstset, stack))
                                    return true;
                            }

                            if (!firstset.Get(t.Group, 1).Contains((RuleLookahead)null))
                                break;
                        }
                    }
                }
            }

            return false;
        }

        private static bool HasDirectLeftRecursion(RuleGroup grp)
        {
            foreach (Rule rule in grp.Rules)
            {
                if (!rule.IsEmpty &&
                       rule.Tokens[0].Type == RuleTokenType.Rule &&
                       rule.Tokens[0].Group == grp)
                {
                    return true;
                }
            }
            return false;
        }

        private static void HandleDirectLeftRecursion(Environment env, RuleGroup grp)
        {
            List<Rule> rulesToRemove = new();
            List<Rule> rulesToAdd = new();
            RuleGroup tail = null;

            // Continue until no direct left recursion is left
            while (HasDirectLeftRecursion(grp))
            {
                if (tail == null)
                {
                    tail = new RuleGroup(env.Groups.Count, grp.Name + "__tail", grp.ReturnType);
                    env.Groups.Add(tail);

                    // Add empty rule
                    Rule emptyRule = new Rule(env.Rules.Count, tail, "");
                    env.Rules.Add(emptyRule);
                    tail.Rules.Add(emptyRule);
                }

                foreach (Rule rule in grp.Rules)
                {
                    // Rule starting with the group nonterminal
                    if (!rule.IsEmpty &&
                        rule.Tokens[0].Type == RuleTokenType.Rule &&
                        rule.Tokens[0].Group == grp)
                    {
                        rulesToRemove.Add(rule);

                        if (rule.Tokens.Count == 1) // Infinite loop!
                            continue;

                        IEnumerable<RuleToken> alpha = rule.Tokens.Skip(1);

                        Rule newRule = new Rule(env.Rules.Count, tail, rule.Code);
                        foreach (RuleToken t in alpha)
                        {
                            if (t.Type == RuleTokenType.Token)
                                newRule.Tokens.Add(env.TokenByName(t.Name));
                            else
                            {
                                RuleToken nt = new RuleToken(0, t.Type, t.Name, t.ReturnType, t.CodeIdentifier);
                                nt.Parent = newRule;
                                nt.IsComplex = t.IsComplex;
                                nt.Group = t.Group;
                                newRule.Tokens.Add(nt);
                            }
                        }

                        RuleToken tailt = new RuleToken(0, RuleTokenType.Rule, tail.Name, "/*TODO*/", "");
                        tailt.Parent = newRule;
                        tailt.Group = tail;
                        newRule.Tokens.Add(tailt);

                        // As we modify the tail, we can directly change it here
                        env.Rules.Add(newRule);
                        tail.Rules.Add(newRule);
                    }
                    else
                    // Rule NOT starting with the group nonterminal
                    {
                        rulesToRemove.Add(rule);
                        Rule newRule = new Rule(env.Rules.Count, grp, rule.Code);
                        foreach (RuleToken t in rule.Tokens)
                        {
                            if (t.Type == RuleTokenType.Token)
                                newRule.Tokens.Add(env.TokenByName(t.Name));
                            else
                            {
                                RuleToken nt = new RuleToken(0, t.Type, t.Name, t.ReturnType, t.CodeIdentifier);
                                nt.Parent = newRule;
                                nt.IsComplex = t.IsComplex;
                                nt.Group = t.Group;
                                newRule.Tokens.Add(nt);
                            }
                        }

                        RuleToken tailt = new RuleToken(0, RuleTokenType.Rule, tail.Name, "/*TODO*/", "");
                        tailt.Parent = newRule;
                        tailt.Group = tail;
                        newRule.Tokens.Add(tailt);

                        rulesToAdd.Add(newRule);
                    }
                }

                // Handle out of loop list changes
                foreach (Rule rule in rulesToRemove)
                {
                    env.Rules.Remove(rule);
                    grp.Rules.Remove(rule);
                }

                foreach (Rule rule in rulesToAdd)
                {
                    env.Rules.Add(rule);
                    grp.Rules.Add(rule);
                }

                rulesToRemove.Clear();
                rulesToAdd.Clear();
            }
        }

        private static void HandleIndirectLeftRecursion(Environment env, RuleGroup grp)
        {
            List<Rule> rulesToRemove = new();
            List<Rule> rulesToAdd = new();

            // Fix indirect left recursion until nothing changes
            while (true)
            {
                foreach (Rule oldRule in grp.Rules)
                {
                    if (!oldRule.IsEmpty &&
                        oldRule.Tokens[0].Type == RuleTokenType.Rule &&
                        oldRule.Tokens[0].Group.ID < grp.ID)
                    {
                        IEnumerable<RuleToken> beta = oldRule.Tokens.Skip(1);
                        rulesToRemove.Add(oldRule);

                        RuleGroup otherGrp = oldRule.Tokens[0].Group;
                        foreach (Rule rule in otherGrp.Rules)
                        {
                            Rule newRule = new Rule(env.Rules.Count, grp, rule.Code);

                            foreach (RuleToken t in rule.Tokens.Concat(beta))
                            {
                                if (t.Type == RuleTokenType.Token)
                                    newRule.Tokens.Add(env.TokenByName(t.Name));
                                else
                                {
                                    RuleToken nt = new RuleToken(0, t.Type, t.Name, t.ReturnType, t.CodeIdentifier);
                                    nt.Parent = newRule;
                                    nt.Group = t.Group;
                                    nt.IsComplex = t.IsComplex;
                                    newRule.Tokens.Add(nt);
                                }
                            }

                            rulesToAdd.Add(newRule);
                        }
                    }
                }

                // Handle out of loop list changes
                if (rulesToRemove.Count > 0)
                {
                    foreach (Rule rule in rulesToRemove)
                    {
                        env.Rules.Remove(rule);
                        grp.Rules.Remove(rule);
                    }

                    foreach (Rule rule in rulesToAdd)
                    {
                        env.Rules.Add(rule);
                        grp.Rules.Add(rule);
                    }

                    rulesToRemove.Clear();
                    rulesToAdd.Clear();
                }
                else
                {
                    break;
                }
            }
        }

        // Fix
        public static void FixLeftRecursion(Environment input)
        {
            // Make sure it is properly sorted
            input.SortByTopologicalOrder();

            // Copy such that newly created "tails" are not iterated
            List<RuleGroup> groups = new(input.Groups);
            foreach (RuleGroup grp in groups)
            {
                HandleIndirectLeftRecursion(input, grp);
                HandleDirectLeftRecursion(input, grp);
            }

            // Make sure it is properly sorted afterwards
            input.SortByTopologicalOrder();
        }

    }
}
