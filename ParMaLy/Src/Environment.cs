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
        List<string> _Tokens = new List<string>();

        public List<Rule> Rules { get { return _Rules; } }
        public List<RuleGroup> Groups { get { return _Groups; } }
        public List<string> Tokens { get { return _Tokens; } }

        public RuleGroup Start { get { return _Start; } }

        public Environment(Logger logger)
        {
            _Logger = logger;
        }

        public void Parse(string source)
        {
            Parser.Parser parser = new Parser.Parser(source, _Logger);
            var tree = parser.Parse();

            //First pass tokens
            foreach (Parser.Statement stmt in tree.Statements)
            {
                if (stmt.Type == Parser.StatementType.TokenDef)
                {
                    Parser.TokenDefStatement tds = stmt as Parser.TokenDefStatement;

                    if (_Tokens.Contains(tds.Token))
                        _Logger.Log(LogLevel.Warning, "Token '" + tds.Token + "' already defined.");
                    else
                        _Tokens.Add(tds.Token);
                }
            }

            //Second pass rules
            foreach (Parser.Statement stmt in tree.Statements)
            {
                if (stmt.Type == Parser.StatementType.Rule)
                {
                    Parser.RuleStatement rs = stmt as Parser.RuleStatement;

                    foreach (Parser.RuleDef def in rs.Rules)
                    {
                        RuleGroup grp = GroupByName(def.Name);
                        if (grp == null)
                        {
                            grp = new RuleGroup(_Groups.Count + 1, def.Name);
                            _Groups.Add(grp);
                        }

                        Rule rule = new Rule(_Rules.Count + 1, grp);

                        foreach (Parser.RuleDefToken t in def.Tokens)
                        {
                            if (t.WasString)
                            {
                                if (!_Tokens.Contains(t.Name))
                                    _Tokens.Add(t.Name);
                            }

                            rule.Tokens.Add(new RuleToken(rule,
                                _Tokens.Contains(t.Name) ? RuleTokenType.Token : RuleTokenType.Rule,
                                t.Name));
                        }

                        if (!IsRuleUnique(rule))
                            _Logger.Log(LogLevel.Warning, "Exact same rule '" + grp.Name + "' already defined.");

                        grp.Rules.Add(rule);
                        _Rules.Add(rule);
                    }
                }
            }

            // Third pass start token
            foreach (Parser.Statement stmt in tree.Statements)
            {
                if (stmt.Type == Parser.StatementType.StartDef)
                {
                    Parser.StartDefStatement sds = stmt as Parser.StartDefStatement;

                    if (_Start != null)
                        _Logger.Log(LogLevel.Warning, "Start rule '" + _Start.Name + "' already defined.");

                    RuleGroup grp = GroupByName(sds.Token);

                    if (grp == null)
                        _Logger.Log(LogLevel.Warning, "Start rule '" + sds.Token + "' not found.");
                    else
                        _Start = grp;
                }
            }
        }

        public List<RuleToken> ParseLine(string source)
        {
            Parser.Parser parser = new Parser.Parser(source, _Logger);
            var tokens = parser.ParseLine();

            List<RuleToken> rt = new List<RuleToken>();
            foreach (Parser.RuleDefToken t in tokens)
            {
                if (t.WasString && !_Tokens.Contains(t.Name))
                {
                    _Logger.Log(LogLevel.Warning, "Token '" + t.Name + "' was not defined in grammar.");
                    break;
                }

                rt.Add(new RuleToken(null,
                    _Tokens.Contains(t.Name) ? RuleTokenType.Token : RuleTokenType.Rule,
                    t.Name));
            }

            return rt;
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
                                other.Tokens[i].String != rule.Tokens[i].String)
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
