using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PML
{
    public static class FirstSet
    {
        // null is Empty!
        public static void Setup(Environment env)
        {
            // First rule (2)
            foreach (Rule r in env.Rules)
            {
                if (r.Group.FirstSet == null)
                    r.Group.FirstSet = new List<string>();

                if (r.IsEmpty)
                {
                    if (!r.Group.FirstSet.Contains(null))
                        r.Group.FirstSet.Add(null);
                }
                else if (r.Tokens.Count == 1 && r.Tokens[0].Type == RuleTokenType.Token)
                {
                    RuleToken t = r.Tokens[0];
                    if (!r.Group.FirstSet.Contains(t.String))
                        r.Group.FirstSet.Add(t.String);
                }
            }

            // Second rule (3)
            Rule3(env, env.Rules, new Stack<RuleGroup>());
        }

        static void Rule3(Environment env, List<Rule> rules, Stack<RuleGroup> stack)
        {
            foreach (Rule r in rules)
            {
                    if (r.Group.FirstSet == null)
                        r.Group.FirstSet = new List<string>();

                    bool empty = true;
                foreach (RuleToken t in r.Tokens)
                {
                    if (t.Type == RuleTokenType.Token)
                    {
                        if (!r.Group.FirstSet.Contains(t.String))
                            r.Group.FirstSet.Add(t.String);

                        empty = false;
                        break;
                    }
                    else
                    {
                        RuleGroup grp = env.GroupByName(t.String);

                        if(!stack.Contains(grp))
                            Rule3(env, grp.Rules, stack);

                        bool somethingNew = false;
                        foreach (string s in grp.FirstSet)
                        {
                            if (s != null && !r.Group.FirstSet.Contains(s))
                            {
                                r.Group.FirstSet.Add(s);
                                somethingNew = true;
                            }
                        }

                        if (!somethingNew)
                        {
                            stack.Push(grp);
                        }

                        if (!grp.FirstSet.Contains(null))
                        {
                            empty = false;
                            break;
                        }
                    }
                }

                if (empty)
                {
                    if (!r.Group.FirstSet.Contains(null))
                        r.Group.FirstSet.Add(null);
                }
            }
        }

        public static List<string> Generate(Environment env, string source)
        {
            return Generate(env, env.ParseLine(source));
        }

        public static List<string> Generate(Environment env, List<RuleToken> tokens)
        {
            return Generate(env, tokens,
                new Dictionary<RuleToken, List<string>>(),
                new Stack<string>());
        }

        public static List<string> Generate(Environment env, RuleToken token)
        {
            return Generate(env, token,
                new Dictionary<RuleToken, List<string>>(),
                new Stack<string>());
        }

        static List<string> Generate(Environment env, List<RuleToken> tokens,
            Dictionary<RuleToken, List<string>> parsed, Stack<string> parents)
        {
            List<string> output = new List<string>();
            bool empty = true;

            foreach(RuleToken token in tokens)
            {
                if (parents.Contains(token.String))
                    continue;

                var other = Generate(env, token, parsed, parents);

                foreach (string str in other)
                {
                    if (str != null && !output.Contains(str))
                        output.Add(str);
                    else if (str == null)
                        empty = false;
                }

                if (!empty)
                {
                    break;
                }
            }

            if (empty)
                output.Add(null);

            return output;
        }

        static List<string> Generate(Environment env, RuleToken token,
            Dictionary<RuleToken, List<string>> parsed, Stack<string> parents)
        {
            if (parsed.ContainsKey(token))
                return parsed[token];

            parents.Push(token.String);

            List<string> output = new List<string>();
            if (token.Type == RuleTokenType.Token)
            {
                output.Add(token.String);
            }
            else
            {
                foreach(Rule r in env.GroupByName(token.String).Rules)
                {
                    if(r.IsEmpty)
                    {
                        if (!output.Contains(null))
                            output.Add(null);
                    }
                    else
                    {
                        var other = Generate(env, r.Tokens, parsed, parents);

                        foreach(string str in other)
                        {
                            if (!output.Contains(str))
                                output.Add(str);
                        }
                    }
                }
            }

            parsed[token] = output;
            parents.Pop();
            return output;
        }
    }
}
