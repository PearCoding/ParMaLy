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
