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
    public static class FollowSet
    {
        // null means End of File!
        public static void Setup(Environment env)
        {
            // First rule (1)
            // Add EOF token to starting rule.
            if(env.Start != null)
            {
                if (env.Start.FollowSet == null)
                    env.Start.FollowSet = new List<string>();
                env.Start.FollowSet.Add(null);
            }

            // Second rule (2)
            foreach (Rule r in env.Rules)
            {
                if (r.Group.FollowSet == null)
                    r.Group.FollowSet = new List<string>();

                for (int i = 0; i < r.Tokens.Count - 1; ++i)
                {
                    RuleToken t = r.Tokens[i];
                    if (t.Type == RuleTokenType.Rule)
                    {
                        RuleGroup grp = t.Group;
                        if (grp.FollowSet == null)
                            grp.FollowSet = new List<string>();

                        var l = FirstSet.Generate(r.Tokens.GetRange(i + 1, r.Tokens.Count - i - 1));

                        foreach (string str in l)
                        {
                            if (!grp.FollowSet.Contains(str))
                                grp.FollowSet.Add(str);
                        }
                    }
                }
            }
            
            // Third rule (3)
            foreach (Rule r in env.Rules)
            {
                for (int i = 0; i < r.Tokens.Count; ++i)
                {
                    RuleToken t = r.Tokens[i];
                    if (t.Type == RuleTokenType.Rule)
                    {
                        RuleGroup grp = t.Group;
                        if (i == r.Tokens.Count - 1)
                        {
                            foreach (string str in r.Group.FollowSet)
                            {
                                if (!grp.FollowSet.Contains(str))
                                    grp.FollowSet.Add(str);
                            }
                        }
                        else
                        {
                            var l = FirstSet.Generate(r.Tokens.GetRange(i + 1, r.Tokens.Count - i - 1));

                            if (l.Contains(null))
                            {
                                foreach (string str in r.Group.FollowSet)
                                {
                                    if (!grp.FollowSet.Contains(str))
                                        grp.FollowSet.Add(str);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
