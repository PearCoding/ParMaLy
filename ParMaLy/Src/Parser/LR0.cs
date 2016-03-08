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

namespace PML.Parser
{
    public class LR0
    {
        List<RuleState> _States = new List<RuleState>();
        public List<RuleState> States { get { return _States; } }

        public LR0()
        {
        }

        public void GenerateStates(Environment env)
        {
            _States.Clear();

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            var l = new List<RuleConfiguration>();
            foreach (Rule r in env.Start.Rules)
            {
                l.Add(new RuleConfiguration(r, -1));
            }
            StepState(env, l);
        }

        void GenerateState(Environment env, Rule r, RuleState state, int p)
        {
            RuleConfiguration conf = new RuleConfiguration(r, p);
            state.Configurations.Add(conf);
            
            if(!conf.IsLast)
            {
                RuleToken t = conf.GetNext();

                if (t.Type != RuleTokenType.Token)
                {
                    RuleGroup g = env.GroupByName(t.Name);

                    foreach (Rule r2 in g.Rules)
                    {
                        bool found = false;
                        foreach (RuleConfiguration c in state.Configurations)
                        {
                            if(c.Rule == r2 && c.Pos == 0)
                            {
                                found = true;
                                break;
                            }
                        }

                        if(!found)
                        {
                            GenerateState(env, r2, state, 0);
                        }
                    }
                }
            }
        }

        void StepState(Environment env, List<RuleConfiguration> confs)
        {
            RuleState state = new RuleState();

            foreach (RuleConfiguration c in confs)
            {
                if (!c.IsLast)
                {
                    GenerateState(env, c.Rule, state, c.Pos + 1);
                }
            }

            if (!_States.Contains(state))
            {
                _States.Add(state);

                foreach (RuleConfiguration conf in state.Configurations)
                {
                    if (!conf.IsLast)
                    {
                        RuleToken t = conf.GetNext();

                        List<RuleConfiguration> next = new List<RuleConfiguration>();
                        foreach (RuleConfiguration c in state.Configurations)
                        {
                            if (!c.IsLast && c.GetNext().Type == t.Type && c.GetNext().Name == t.Name)
                                next.Add(c);
                        }

                        StepState(env, next);
                    }
                }
            }
        }
    }
}
