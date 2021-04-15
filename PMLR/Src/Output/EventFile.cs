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
using System.IO;
using System.Linq;

namespace PML.Output
{
    static class EventFile
    {
        public static void PrintEvents(TextWriter writer, IEnumerable<Events.IEvent> events)
        {
            int i = 1;
            foreach (Events.IEvent e in events)
            {
                writer.WriteLine("#-------------------------------------------------#");
                PrintEvent(writer, i, e);
                ++i;
            }

            if (events.Count() != 0)
                writer.WriteLine("#-------------------------------------------------#");

            writer.Flush();
        }

        static void PrintEvent(TextWriter writer, int i, Events.IEvent e)
        {
            writer.WriteLine("[" + i + "] Event: " + e.Name);
            if (e is Events.LLRuleEvent)
            {
                Events.LLRuleEvent re = e as Events.LLRuleEvent;
                writer.WriteLine("  Current position: " + re.Position);
                writer.WriteLine("  Current stack: " + string.Join(", ", re.Stack.Select(v => "'" + v.Name + "'").ToArray()));
                writer.WriteLine("  Current lookahead: " + (re.Lookahead == null ? "$" : re.Lookahead.ToString()));
                writer.WriteLine("  Produced: " + re.To.Group.Name + ".[" + re.To.ID + "]");
            }
            else if (e is Events.LLTokenEvent)
            {
                Events.LLTokenEvent te = e as Events.LLTokenEvent;
                writer.WriteLine("  Current position: " + te.Position);
                writer.WriteLine("  Current stack: " + string.Join(", ", te.Stack.Select(v => "'" + v.Name + "'").ToArray()));
                writer.WriteLine("  Current lookahead: " + (te.Lookahead == null ? "$" : te.Lookahead.ToString()));
                writer.WriteLine("  Matched token: '" + te.Match.Name + "'");
            }
            else if (e is Events.LRAcceptEvent)
            {
                Events.LRAcceptEvent ae = e as Events.LRAcceptEvent;
                writer.WriteLine("  Current position: " + ae.Position);
                writer.WriteLine("  Current stack: " + string.Join(", ", ae.Stack.Select(v => v.ToString()).ToArray()));
                writer.WriteLine("  Current lookahead: " + (ae.Lookahead == null ? "$" : ae.Lookahead.ToString()));
            }
            else if (e is Events.LRShiftEvent)
            {
                Events.LRShiftEvent se = e as Events.LRShiftEvent;
                writer.WriteLine("  Current position: " + se.Position);
                writer.WriteLine("  Current stack: " + string.Join(", ", se.Stack.Select(v => v.ToString()).ToArray()));
                writer.WriteLine("  Current lookahead: " + (se.Lookahead == null ? "$" : se.Lookahead.ToString()));
                writer.WriteLine("  Next state: " + se.NextState);
            }
            else if (e is Events.LRReduceEvent)
            {
                Events.LRReduceEvent re = e as Events.LRReduceEvent;
                writer.WriteLine("  Current position: " + re.Position);
                writer.WriteLine("  Current stack: " + string.Join(", ", re.Stack.Select(v => v.ToString()).ToArray()));
                writer.WriteLine("  Current lookahead: " + (re.Lookahead == null ? "$" : re.Lookahead.ToString()));
                writer.WriteLine("  Rule: " + re.Rule.Group.Name + "[" + re.Rule.ID + "]");
            }
        }
    }
}
