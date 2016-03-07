using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PML
{
    public class Environment
    {
        Logger _Logger;
        List<Rule> _Rules = new List<Rule>();
        List<string> _Tokens = new List<string>();

        public List<Rule> Rules { get { return _Rules; } }
        public List<string> Tokens { get { return _Tokens; } }

        public Environment(Logger logger)
        {
            _Logger = logger;
        }

        public void Parse(string source)
        {
            Parser.Parser parser = new Parser.Parser(source, _Logger);
            var tree = parser.Parse();

            //First pass tokens
            foreach(Parser.Statement stmt in tree.Statements)
            {
                if(stmt.Type == Parser.StatementType.TokenDef)
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
                        Rule rule = new Rule(_Rules.Count + 1, def.Name);

                        foreach (Parser.RuleDefToken t in def.Tokens)
                        {
                            if(t.WasString)
                            {
                                if (!_Tokens.Contains(t.Name))
                                    _Tokens.Add(t.Name);
                            }

                            rule.Tokens.Add(new RuleToken(rule,
                                _Tokens.Contains(t.Name) ? RuleTokenType.Token : RuleTokenType.Rule,
                                t.Name));
                        }

                        if (!IsRuleUnique(rule))
                            _Logger.Log(LogLevel.Warning, "Exact same rule '" + rule.Name + "' already defined.");

                        _Rules.Add(rule);
                    }
                }
            }
        }

        public List<Rule> RulesByName(string name)
        {
            List<Rule> rules = new List<Rule>();

            foreach(Rule r in _Rules)
            {
                if (r.Name == name)
                    rules.Add(r);
            }

            return rules;
        }

        bool IsRuleUnique(Rule rule)
        {
            foreach(Rule other in _Rules)
            {
                if(other.Name == rule.Name)
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
