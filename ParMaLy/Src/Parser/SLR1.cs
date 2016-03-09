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
    public class SLR1
    {
        LR0 _LR0 = new Parser.LR0();
        ActionTable _ActionTable = new ActionTable();

        public List<RuleState> States { get { return _LR0.States; } }

        public RuleState StartState { get { return _LR0.StartState; } }

        public ActionTable ActionTable { get { return _ActionTable; } }

        public GotoTable GotoTable { get { return _LR0.GotoTable; } }

        public SLR1()
        {
        }

        public void GenerateStates(Environment env)
        {
            _LR0.GenerateStates(env);
        }
        
        public void GenerateActionTable(Environment env, Logger logger)
        {
            _ActionTable.Clear();

            foreach(RuleState state in States)
            {
                foreach(RuleConfiguration conf in state.Configurations)
                {
                    if(conf.Rule.Group == env.Start && conf.IsLast)
                    {
                        _ActionTable.Set(state, null, ActionTable.Action.Accept, null);
                    }
                    else if(conf.IsLast)//The only difference to LR0 is here!
                    {
                        var follow = conf.Rule.Group.FollowSet;
                        foreach(string t in follow)
                        {
                            _ActionTable.Set(state, t, ActionTable.Action.Reduce, state);
                        }
                    }
                    else if(conf.GetNext().Type == RuleTokenType.Token)
                    {
                        RuleToken next = conf.GetNext();
                        RuleState found = null;
                        foreach(RuleState.Connection c in state.Production)
                        {
                            if(c.Token == next)
                            {
                                if (found != null)//Reduce Reduce Conflict
                                    logger.Log(LogLevel.Warning, "ReduceReduceConflict in state " + state.ID);
                                else
                                    found = c.State;
                            }
                        }

                        _ActionTable.Set(state, next.Name, ActionTable.Action.Shift, found);
                    }
                }
            }
        }

        public void GenerateGotoTable(Environment env, Logger logger)
        {
            _LR0.GenerateGotoTable(env, logger);
        }
    }
}
