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

using System;
using System.Linq;
using System.IO;

namespace PML.Output
{
    static class Grammar
    {
        public static void PrintGrammar(TextWriter writer, Environment env)
        {
            writer.WriteLine("/* Generated with PMLA */");
            writer.WriteLine();

            foreach (var t in env.Tokens)
            {
                if (t.IsComplex)
                {
                    if (String.IsNullOrEmpty(t.ReturnType))
                        writer.WriteLine("%token " + t.Name + ";");
                    else
                        writer.WriteLine("%token<" + t.ReturnType + "> " + t.Name + ";");
                }
            }

            if (env.Start != null)
                writer.WriteLine("%start " + env.Start.Name + ";");

            writer.WriteLine();
            foreach (var grp in env.Groups)
            {
                PrintGroup(writer, grp);
                writer.WriteLine();
            }

            writer.Flush();
        }

        static void PrintGroup(TextWriter writer, RuleGroup grp)
        {
            int length;
            if (String.IsNullOrEmpty(grp.ReturnType))
            {
                writer.Write(grp.Name + ": ");
                length = grp.Name.Length;
            }
            else
            {
                writer.Write(grp.Name + "<" + grp.ReturnType + ">: ");
                length = grp.Name.Length + 2 + grp.ReturnType.Length;
            }

            for (int i = 0; i < grp.Rules.Count; ++i)
            {
                var r = grp.Rules[i];

                if (r.IsEmpty)
                    writer.WriteLine("/* EMPTY */");
                else
                {
                    writer.WriteLine(String.Join(" ",
                        r.Tokens.Select(v => (v.Type == RuleTokenType.Rule || v.IsComplex ? v.Name : "'" + v.Name + "'") + (String.IsNullOrEmpty(v.CodeIdentifier) ? "" : "[" + v.CodeIdentifier + "]")).ToArray()));
                }

                if (!String.IsNullOrEmpty(r.Code))
                {
                    for (int j = 0; j < length; ++j)
                        writer.Write(" ");

                    writer.WriteLine("{" + r.Code + "}");
                }

                for (int j = 0; j < length; ++j)
                    writer.Write(" ");

                if (i != grp.Rules.Count - 1)
                    writer.Write("| ");
            }

            writer.WriteLine(";");
        }
    }
}
