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
using System.Linq;
using System.IO;

namespace PML.Output
{
    using R;

    public static class PseudoCode
    {
        public static void PrintRD(TextWriter writer, IEnumerable<RState> states, Environment env, Style.CodeStyle style)
        {
            foreach (var state in states)
            {
                writer.WriteLine("function " + style.FunctionNamePrefix + state.Group.Name + "() {");
                if (state.Lookaheads.Count == 1)
                {
                    PrintRDRule(writer, state.Lookaheads.First().Key, style.Ident, style);
                }
                else
                {
                    int i = 0;
                    foreach (var p in state.Lookaheads)
                    {
                        if (i != 0)
                            writer.Write(style.Ident + "else ");
                        else
                            writer.Write(style.Ident);

                        writer.Write("if(");

                        if (style.UseInExpression)
                        { 
                            writer.Write("current() in [");
                            writer.Write(string.Join(",", p.Value.Lookaheads.Select(
                                        delegate (RuleLookahead v) {
                                            if (v.Count() != 1)
                                                return "[" + v.Join("", s => (s == null ? style.NullExpression : style.StringBracket + s + style.StringBracket)) + "]";
                                            else
                                                return v[0] == null ? style.NullExpression : style.StringBracket + v[0] + style.StringBracket;
                                        }
                                        ).ToArray()));
                            writer.Write("]) ");
                        }
                        else
                        {
                            writer.Write(string.Join(" ||\n" + style.Ident + style.Ident + style.Ident,
                                p.Value.Lookaheads.Select(
                                           delegate (RuleLookahead v) {
                                           if (v.Count() != 1)
                                                   return "(" + PrintLookaheadIf(writer, v, style) + ")";
                                           else
                                               return "current() == " +
                                               (v[0] == null ? style.NullExpression : style.StringBracket + v[0] + style.StringBracket);
                                           }
                                           ).ToArray()));
                        }

                        writer.WriteLine("{");
                        PrintRDRule(writer, p.Key, style.Ident + style.Ident, style);
                        writer.WriteLine(style.Ident + "}");
                        ++i;
                    }
                    writer.WriteLine(style.Ident + "else {");
                    writer.WriteLine(style.Ident + style.Ident + "error();");
                    writer.WriteLine(style.Ident + "}");
                }
                writer.WriteLine("}");
                writer.WriteLine();
            }
            writer.Flush();
        }

        static void PrintRDRule(TextWriter writer, Rule rule, string prefix, Style.CodeStyle style)
        {
            foreach(var t in rule.Tokens)
            {
                if (t.Type == RuleTokenType.Token)
                    writer.WriteLine(prefix + "accept(" + style.StringBracket + t.Name + style.StringBracket + ");");
                else
                    writer.WriteLine(prefix + style.FunctionNamePrefix + t.Group.Name + "();");
            }
        }

        static string PrintLookaheadIf(TextWriter writer, RuleLookahead lookahead, Style.CodeStyle style)
        {
            string s = "";
            for (int i = 0; i < lookahead.Count(); ++i)
            {
                if (i == 0)
                    s += "current() ";
                else
                    s += "lookahead(" + i + ") ";

                s += " == " + style.StringBracket + lookahead[i] + style.StringBracket;

                if (i != lookahead.Count() - 1)
                    s += " && ";
            }

            return s;
        }
    }
}
