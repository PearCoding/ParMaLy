﻿/*
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

namespace PML.Events
{
    public class LRAcceptEvent : IEvent
    {
        public string Name { get { return "LRAccept"; } }

        public List<int> Stack = new List<int>();
        public RuleLookahead Lookahead;
        public int Position;

        public LRAcceptEvent(IEnumerable<int> stack, RuleLookahead lookahead, int position)
        {
            Stack.AddRange(stack);
            Lookahead = lookahead;
            Position = position;
        }
    }

    public class LRShiftEvent : IEvent
    {
        public string Name { get { return "LRShift"; } }

        public List<int> Stack = new List<int>();
        public RuleLookahead Lookahead;
        public int Position;
        public int NextState;

        public LRShiftEvent(int nextstate, IEnumerable<int> stack,
            RuleLookahead lookahead, int position)
        {
            NextState = nextstate;
            Stack.AddRange(stack);
            Lookahead = lookahead;
            Position = position;
        }
    }

    public class LRReduceEvent : IEvent
    {
        public string Name { get { return "LRReduce"; } }

        public List<int> Stack = new List<int>();
        public RuleLookahead Lookahead;
        public int Position;
        public Rule Rule;

        public LRReduceEvent(Rule rule, IEnumerable<int> stack,
            RuleLookahead lookahead, int position)
        {
            Rule = rule;
            Stack.AddRange(stack);
            Lookahead = lookahead;
            Position = position;
        }
    }
}
