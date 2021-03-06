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

using System.Linq;

namespace PML
{
    public static class FollowSet
    {
        // null means End of File (EOF)!
        public static void Setup(Environment env, FollowSetCache cache, int k)
        {
            // Setup caches
            foreach (RuleGroup grp in env.Groups)
            {
                cache.Set(grp, k, new RuleLookaheadSet());
            }

            // Rule 1: Add EOF to starter group
            if (env.Start != null)
            {
                RuleLookaheadSet set = cache.Get(env.Start, k);
                set.Add(null);
            }

            // Rule 2:
            foreach (Rule r in env.Rules)
            {
                for (int i = 0; i < r.Tokens.Count - 1; ++i)
                {
                    RuleToken t = r.Tokens[i];
                    if (t.Type == RuleTokenType.Rule)
                    {
                        RuleLookaheadSet l = env.FirstCache.Generate(r.Tokens.Skip(i + 1), k);
                        RuleLookaheadSet set = cache.Get(t.Group, k);
                        foreach (RuleLookahead look in l)
                        {
                            if (!set.Contains(look))
                                set.Add(look);
                        }
                    }
                }
            }

            // Rule 3:
            foreach (Rule r in env.Rules)
            {
                for (int i = 0; i < r.Tokens.Count; ++i)
                {
                    RuleToken t = r.Tokens[i];
                    if (t.Type == RuleTokenType.Rule)
                    {
                        RuleLookaheadSet setT = cache.Get(t.Group, k);
                        RuleLookaheadSet setR = cache.Get(r.Group, k);
                        if (i == r.Tokens.Count - 1)
                        {
                            foreach (RuleLookahead look in setR)
                            {
                                setT.AddUnique(look);
                            }
                        }
                        else
                        {
                            RuleLookaheadSet l = env.FirstCache.Generate(r.Tokens.Skip(i + 1), k);

                            if (l.Contains((RuleLookahead)null))
                            {
                                foreach (RuleLookahead look in setR)
                                {
                                    setT.AddUnique(look);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
