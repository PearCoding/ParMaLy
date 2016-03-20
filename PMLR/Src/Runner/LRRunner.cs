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
    //LR runner
    public class LRRunner : IRunner
    {
        public BU.ActionTable ActionTable = new BU.ActionTable();
        public BU.GotoTable GotoTable = new BU.GotoTable();
        public int K;

        public LRRunner(int k)
        {
            K = k;
        }

        public IEnumerable<Events.IEvent> Run(TokenLexer lexer, Environment env, Logger logger)
        {
            List<Events.IEvent> events = new List<Events.IEvent>();

            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (true)
            {
                var look = (lexer.Left > 0 ? lexer.Current(env, K <= 1 ? 0 : (K - 1)) : null);
                var action = ActionTable.Get(stack.Peek(), look);

                //if (look != null)
                //{
                //    for (int i = 1; i < K; ++i)
                //    {
                //        if (action == null)
                //        {
                //            look = new RuleLookahead(look.Take(look.Count - 1));
                //            action = ActionTable.Get(stack.Peek(), look);
                //        }
                //    }
                //}

                if(action == null)
                {
                    logger.Log(LogLevel.Error, "Parser Error: State " + stack.Peek() + " with lookahead "
                        + (look != null ? "[" + look.Join(",") + "]" : "$"));
                    break;
                }
                else if(action.Action == BU.ActionTable.Action.Accept)
                {
                    events.Add(new Events.LRAcceptEvent(stack, look, lexer.Position));
                    break;
                }
                else if(action.Action == BU.ActionTable.Action.Shift)
                {
                    events.Add(new Events.LRShiftEvent(action.StateID, stack, look, lexer.Position));
                    stack.Push(action.StateID);
                    lexer.Step();
                }
                else//Reduce
                {
                    var rule = env.RuleByID(action.StateID);
                    events.Add(new Events.LRReduceEvent(rule, stack, look, lexer.Position));

                    for (int i = 0; i < rule.Tokens.Count; ++i)
                        stack.Pop();

                    var go = GotoTable.Get(stack.Peek(), rule.Group);

                    if(go != null)
                        stack.Push(go.StateID);
                    else
                        logger.Log(LogLevel.Error, "Parser Error: No GoTo for State " + stack.Peek() + " with lookahead "
                            + (look != null ? "[" + look.Join(",") + "]" : "$"));
                }
            }

            if(lexer.Left != 0)
                logger.Log(LogLevel.Error, "Parser did not finished properly.");

            return events;
        }
    }
}
