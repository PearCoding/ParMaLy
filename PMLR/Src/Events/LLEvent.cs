using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML.Events
{
    public class LLEvent : IEvent
    {
        public string Name { get { return "LL"; } }

        public List<RuleToken> Stack = new List<RuleToken>();
        public RuleLookahead Lookahead;
        public int Position;

        public LLEvent(IEnumerable<RuleToken> stack, RuleLookahead lookahead, int position)
        {
            Stack.AddRange(stack);
            Lookahead = lookahead;
            Position = position;
        }
    }
}
