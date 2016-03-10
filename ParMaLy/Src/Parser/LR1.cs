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
    public class LR1 : BTParser
    {
        List<RuleState> _States = new List<RuleState>();
        RuleState _StartState;
        ActionTable _ActionTable = new ActionTable();
        GotoTable _GotoTable = new GotoTable();

        public List<RuleState> States { get { return _States; } }

        public RuleState StartState { get { return _StartState; } }

        public ActionTable ActionTable { get { return _ActionTable; } }

        public GotoTable GotoTable { get { return _GotoTable; } }

        public LR1()
        {
        }

        public void GenerateStates(Environment env, Logger logger)
        {
            _States.Clear();
            _StartState = null;

            // We can not start without a 'Start' token.
            if (env.Start == null || env.Start.Rules.Count == 0)
                return;

            LR0 lr0 = new LR0();
            lr0.GenerateStates(env, logger);
            List<RuleState> lr0States = lr0.States;

            ExtractState(lr0.StartState, logger);           
        }

        void ExtractState(RuleState lr0State, Logger logger)
        {
            var dict = BigFollowSet(lr0State);
            RuleState lr1State = new RuleState(0);
            _StartState = lr1State;

            foreach (var conf0 in lr0State.Configurations)
            {
                if (dict.ContainsKey(conf0.Rule.Group))
                {
                    var l = dict[conf0.Rule.Group];

                    foreach (var s in l)
                    {
                        var conf = new RuleConfiguration(conf0.Rule, conf0.Pos, new RuleLookahead(s));
                        lr1State.Configurations.Add(conf);
                    }
                }
                else
                {
                    logger.Log(LogLevel.Error, "Configuration rule group should be in (big) follow set, but isn't.");
                }
            }
            _States.Add(lr1State);

            StepState(lr1State, logger);
        }

        void GenerateClosure(RuleState state, Logger logger)
        {

        }

        void StepState(RuleState state, Logger logger)
        {

        }

        Dictionary<RuleGroup, List<string>> BigFollowSet(RuleState state)
        {
            Dictionary<RuleGroup, List<string>> sets = new Dictionary<RuleGroup, List<string>>();

            foreach(var conf in state.Configurations)
            {
                if (!sets.ContainsKey(conf.Rule.Group))
                {
                    var list = new List<string>();
                    
                    foreach (var conf2 in state.Configurations)
                    {
                        if (!conf2.IsLast &&
                            conf2.GetNext().Type == RuleTokenType.Rule &&
                            conf2.GetNext().Name == conf.Rule.Group.Name)
                        {
                            foreach (var s in conf2.Rule.Group.FollowSet)
                            {
                                if(!list.Contains(s))
                                    list.Add(s);
                            }
                        }
                    }

                    sets[conf.Rule.Group] = list;
                }
            }

            return sets;
        }

        public void GenerateActionTable(Environment env, Logger logger)
        {
            _ActionTable.Clear();

            foreach(RuleState state in _States)
            {
                foreach(RuleConfiguration conf in state.Configurations)
                {
                    if(conf.Rule.Group == env.Start && conf.IsLast)
                    {
                        _ActionTable.Set(state, null, ActionTable.Action.Accept, null);
                    }
                    else if(conf.IsLast)
                    {
                        foreach(string t in env.Tokens)
                        {
                            _ActionTable.Set(state, t, ActionTable.Action.Reduce, state);
                        }
                        _ActionTable.Set(state, null, ActionTable.Action.Reduce, state);
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
            _GotoTable.Clear();

            foreach (RuleState state in _States)
            {
                foreach (var c in state.Production)
                {
                    if(c.Token.Type == RuleTokenType.Rule)
                    {
                        _GotoTable.Set(state, env.GroupByName(c.Token.Name), c.State);
                    }
                }
            }
        }
    }
}
