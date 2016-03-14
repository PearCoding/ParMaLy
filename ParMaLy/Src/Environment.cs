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
    public class Environment
    {
        Logger _Logger;
        List<Rule> _Rules = new List<Rule>();
        RuleGroup _Start;
        List<RuleGroup> _Groups = new List<RuleGroup>();
        List<RuleToken> _Tokens = new List<RuleToken>();

        public List<Rule> Rules { get { return _Rules; } }
        public List<RuleGroup> Groups { get { return _Groups; } }
        public List<RuleToken> Tokens { get { return _Tokens; } }

        public RuleGroup Start { get { return _Start; } }

        public Environment(Logger logger)
        {
            _Logger = logger;
        }

        public void Parse(string source)
        {
            Grammar.Parser parser = new Grammar.Parser(source, _Logger);
            var tree = parser.Parse();

            //First pass tokens
            foreach (Grammar.Statement stmt in tree.Statements)
            {
                if (stmt.Type == Grammar.StatementType.TokenDef)
                {
                    Grammar.TokenDefStatement tds = stmt as Grammar.TokenDefStatement;

                    RuleToken token = new RuleToken(RuleTokenType.Token, tds.Token);
                    token.IsComplex = true;

                    if (_Tokens.Contains(token))
                        _Logger.Log(LogLevel.Warning, "Token '" + tds.Token + "' already defined.");
                    else
                        _Tokens.Add(token);
                }
            }

            //Second pass rules
            foreach (Grammar.Statement stmt in tree.Statements)
            {
                if (stmt.Type == Grammar.StatementType.Rule)
                {
                    Grammar.RuleStatement rs = stmt as Grammar.RuleStatement;

                    foreach (Grammar.RuleDef def in rs.Rules)
                    {
                        RuleGroup grp = GroupByName(def.Name);
                        if (grp == null)
                        {
                            grp = new RuleGroup(_Groups.Count + 1, def.Name);
                            _Groups.Add(grp);
                        }

                        Rule rule = new Rule(_Rules.Count + 1, grp);

                        foreach (Grammar.RuleDefToken t in def.Tokens)
                        {
                            if (t.WasString)
                            {
                                var token = new RuleToken(RuleTokenType.Token, t.Name);
                                if (!_Tokens.Contains(token))
                                    _Tokens.Add(token);

                                rule.Tokens.Add(token);
                            }
                            else
                            {
                                var token = new RuleToken(RuleTokenType.Token, t.Name);
                                int index = _Tokens.IndexOf(token);

                                if (index < 0)
                                {
                                    token = new RuleToken(RuleTokenType.Rule, t.Name);
                                    token.Parent = rule;
                                    rule.Tokens.Add(token);
                                }
                                else
                                    rule.Tokens.Add(_Tokens[index]);
                            }
                        }

                        if (!IsRuleUnique(rule))
                            _Logger.Log(LogLevel.Warning, "Exact same rule '" + grp.Name + "' already defined.");

                        grp.Rules.Add(rule);
                        _Rules.Add(rule);
                    }
                }
            }

            // Third pass start token
            foreach (Grammar.Statement stmt in tree.Statements)
            {
                if (stmt.Type == Grammar.StatementType.StartDef)
                {
                    Grammar.StartDefStatement sds = stmt as Grammar.StartDefStatement;

                    if (_Start != null)
                        _Logger.Log(LogLevel.Warning, "Start rule '" + _Start.Name + "' already defined.");

                    RuleGroup grp = GroupByName(sds.Token);

                    if (grp == null)
                        _Logger.Log(LogLevel.Warning, "Start rule '" + sds.Token + "' not found.");
                    else
                        _Start = grp;
                }
            }

            // Fourth pass: Set groups in tokens
            foreach(var r in _Rules)
            {
                foreach(var t in r.Tokens)
                {
                    if(t.Type == RuleTokenType.Rule)
                    {
                        RuleGroup grp = GroupByName(t.Name);
                        if(grp != null)
                        {
                            t.Group = grp;
                        }
                        else
                        {
                            _Logger.Log(LogLevel.Warning, "Unknown rule '" + t.Name + "' in rule " + r.Group.Name);
                        }
                    }
                }
            }
        }
        

        // Access
        public RuleGroup GroupByName(string name)
        {
            foreach(RuleGroup grp in _Groups)
            {
                if (grp.Name == name)
                    return grp;
            }

            return null;
        }

        public Rule RuleByID(int id)
        {
            foreach(Rule r in _Rules)
            {
                if (r.ID == id)
                    return r;
            }
            return null;
        }

        // Utils
        bool IsRuleUnique(Rule rule)
        {
            foreach(Rule other in _Rules)
            {
                if(other.Group == rule.Group)
                {
                    if(other.Tokens.Count == rule.Tokens.Count)
                    {
                        bool found = false;
                        for(int i = 0; i < rule.Tokens.Count; ++i)
                        {
                            if(other.Tokens[i].Type != rule.Tokens[i].Type ||
                                other.Tokens[i].Name != rule.Tokens[i].Name)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            return false;    
                    }
                }
            }

            return true;
        }
    }
}
