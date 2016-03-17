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

namespace PML.Runner
{
    //LL(k) runner
    public class LLRunner : IRunner
    {
        public TD.LookupTable Table = new TD.LookupTable();
        public int K;

        public LLRunner(int k)
        {
            K = k;
        }

        public IEnumerable<Events.IEvent> Run(TokenLexer lexer, Environment env, Logger logger)
        {    
            return K < 1 ? Run0(lexer, env, logger) : RunK(lexer, env, logger);
        }

        IEnumerable<Events.IEvent> Run0(TokenLexer lexer, Environment env, Logger logger)
        {
            List<Events.IEvent> events = new List<Events.IEvent>();

            Stack<RuleToken> stack = new Stack<RuleToken>();
            var start = new RuleToken(0, RuleTokenType.Rule, env.Start.Name);
            start.Group = env.Start;
            stack.Push(start);

            while (lexer.Left > 0 || stack.Count > 0)
            {
                var t = stack.Pop();

                if (lexer.Current(env)[0] == t)
                {
                    lexer.Step();

                    events.Add(new Events.LLTokenEvent(t, stack, null, lexer.Position));
                }
                else if (t.Type == RuleTokenType.Token)
                {
                    logger.Log(LogLevel.Error, "Did not expect token '" + lexer.Current(env)[0].Name + "', expected '" + t.Name + "'");
                    break;
                }
                else
                {
                    if (t.Group.Rules.Count == 1)
                    {
                        events.Add(new Events.LLRuleEvent(t.Group.Rules[0], stack, null, lexer.Position));

                        foreach (var nt in t.Group.Rules[0].Tokens.Reverse<RuleToken>())
                        {
                            stack.Push(nt);
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Error, "CFG not suitable for K = 0!");
                        break;
                    }
                }
            }

            if (lexer.Left != 0)
                logger.Log(LogLevel.Error, "Parser did not finished properly.");

            return events;
        }

        IEnumerable<Events.IEvent> RunK(TokenLexer lexer, Environment env, Logger logger)
        {
            List<Events.IEvent> events = new List<Events.IEvent>();

            Stack<RuleToken> stack = new Stack<RuleToken>();
            var start = new RuleToken(0, RuleTokenType.Rule, env.Start.Name);
            start.Group = env.Start;
            stack.Push(start);

            while (lexer.Left > 0 && stack.Count > 0)
            {
                var t = stack.Pop();

                if (lexer.Current(env)[0] == t)
                {
                    lexer.Step();

                    events.Add(new Events.LLTokenEvent(t, stack, lexer.Current(env, K - 1), lexer.Position));
                }
                else if (t.Type == RuleTokenType.Token)
                {
                    logger.Log(LogLevel.Error, "Did not expect token '" + lexer.Current(env)[0].Name + "', expected '" + t.Name + "'");
                    break;
                }
                else
                {
                    var look = lexer.Current(env, K - 1);
                    if (Table.Has(t.Group, look))
                    {
                        var e = Table.Get(t.Group, look);
                        events.Add(new Events.LLRuleEvent(e.Rule, stack, look, lexer.Position));

                        foreach (var nt in e.Rule.Tokens.Reverse<RuleToken>())
                        {
                            stack.Push(nt);
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Error, "Parser Error: No lookup entry for " + look.ToString() + " and " + t.Name);
                        break;
                    }
                }
            }

            if (lexer.Left != 0)
                logger.Log(LogLevel.Error, "Parser did not finish properly.");

            return events;
        }
    }
}
