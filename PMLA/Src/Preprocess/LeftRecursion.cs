﻿/*
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

            foreach(var rule in current.Rules)
            {
                foreach(var t in rule.Tokens)
                {
                    if (t.Type == RuleTokenType.Token)
                        break;
                    else
                    {
                        if (t.Group == search)
                            return true;
                        else
                        {
                            if(!stack.Contains(t.Group))
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

        // Fix
        public static Environment FixLeftRecursion(Environment input)
        {
            Environment output = new Environment(input.Logger);
            output.Tokens.AddRange(input.Tokens);

            foreach(var oldGrp in input.Groups)
            {
                var newGrp = new RuleGroup(output.Groups.Count, oldGrp.Name);
                output.Groups.Add(newGrp);

                if (input.Start == oldGrp)
                    output.Start = newGrp;

                // Remove indirect recursion
                foreach (var oldRule in oldGrp.Rules)
                {
                    if (!oldRule.IsEmpty &&
                        oldRule.Tokens[0].Type == RuleTokenType.Rule &&
                        oldRule.Tokens[0].Group.ID < oldGrp.ID)
                    {
                        var beta = oldRule.Tokens.Skip(1);
                        // We can not use the same ids... It can change in the future!
                        var otherGrp = input.GroupByName(oldRule.Tokens[0].Group.Name);

                        foreach(var rule in otherGrp.Rules)
                        {
                            var newRule = new Rule(output.Rules.Count, newGrp);

                            foreach (var t in rule.Tokens.Concat(beta))
                            {
                                if (t.Type == RuleTokenType.Token)
                                    newRule.Tokens.Add(output.TokenByName(t.Name));
                                else
                                {
                                    var nt = new RuleToken(0, t.Type, t.Name);
                                    nt.Parent = newRule;
                                    newRule.Tokens.Add(nt);
                                }
                            }

                            output.Rules.Add(newRule);
                            newGrp.Rules.Add(newRule);
                        }
                    }
                }

                // Remove direct recursion
                bool hasDirectRecursion = false;
                foreach (var oldRule in oldGrp.Rules)
                {
                    if (!oldRule.IsEmpty &&
                           oldRule.Tokens[0].Type == RuleTokenType.Rule &&
                           oldRule.Tokens[0].Group == oldGrp)
                    {
                        hasDirectRecursion = true;
                        break;
                    }
                }

                if (hasDirectRecursion)
                {
                    RuleGroup tail = new RuleGroup(output.Groups.Count, oldGrp.Name + "__tail");
                    output.Groups.Add(tail);
                    Rule emptyRule = new Rule(output.Rules.Count, tail);
                    output.Rules.Add(emptyRule);
                    tail.Rules.Add(emptyRule);

                    foreach (var oldRule in oldGrp.Rules)
                    {
                        if (!oldRule.IsEmpty &&
                            oldRule.Tokens[0].Type == RuleTokenType.Rule &&
                            oldRule.Tokens[0].Group == oldGrp)
                        {
                            var alpha = oldRule.Tokens.Skip(1);

                            var newRule = new Rule(output.Rules.Count, tail);
                            foreach (var t in alpha)
                            {
                                if (t.Type == RuleTokenType.Token)
                                    newRule.Tokens.Add(output.TokenByName(t.Name));
                                else
                                {
                                    var nt = new RuleToken(0, t.Type, t.Name);
                                    nt.Parent = newRule;
                                    newRule.Tokens.Add(nt);
                                }
                            }

                            var tailt = new RuleToken(0, RuleTokenType.Rule, tail.Name);
                            tailt.Parent = newRule;
                            newRule.Tokens.Add(tailt);

                            output.Rules.Add(newRule);
                            tail.Rules.Add(newRule);
                        }
                        else if (!oldRule.IsEmpty &&
                            oldRule.Tokens[0].Type == RuleTokenType.Rule &&
                            oldRule.Tokens[0].Group.ID < oldGrp.ID)
                        {
                            // Ignore it
                            // Will be handled by the indirect recursion path.
                        }
                        else
                        {
                            var newRule = new Rule(output.Rules.Count, newGrp);
                            foreach (var t in oldRule.Tokens)
                            {
                                if (t.Type == RuleTokenType.Token)
                                    newRule.Tokens.Add(output.TokenByName(t.Name));
                                else
                                {
                                    var nt = new RuleToken(0, t.Type, t.Name);
                                    nt.Parent = newRule;
                                    newRule.Tokens.Add(nt);
                                }
                            }

                            var tailt = new RuleToken(0, RuleTokenType.Rule, tail.Name);
                            tailt.Parent = newRule;
                            newRule.Tokens.Add(tailt);

                            output.Rules.Add(newRule);
                            newGrp.Rules.Add(newRule);
                        }
                    }
                }
                else
                {
                    foreach (var oldRule in oldGrp.Rules)
                    {
                        var newRule = new Rule(output.Rules.Count, newGrp);
                        foreach (var t in oldRule.Tokens)
                        {
                            if (t.Type == RuleTokenType.Token)
                                newRule.Tokens.Add(output.TokenByName(t.Name));
                            else
                            {
                                var nt = new RuleToken(0, t.Type, t.Name);
                                nt.Parent = newRule;
                                newRule.Tokens.Add(nt);
                            }
                        }
                        output.Rules.Add(newRule);
                        newGrp.Rules.Add(newRule);
                    }
                }
            }//End of groups loop

            // Set groups in tokens
            foreach (var r in output.Rules)
            {
                foreach (var t in r.Tokens)
                {
                    if (t.Type == RuleTokenType.Rule)
                    {
                        t.Group = output.GroupByName(t.Name);
                    }
                }
            }
            return output;
        }
    }
}
