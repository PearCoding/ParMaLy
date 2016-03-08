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
using System.IO;
using System.Collections.Generic;

namespace PML
{
    using NDesk.Options;

    class Options
    {
        public string GroupDotFile;
        public string BreakdownFile;

        public string Parser;
        public string StateFile;

        public bool ShowHelp = false;
    }

    class Program
    {
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: ParMaLy grammar_file [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Main(string[] args)
        {
            Options opts = new Options();
            
            OptionSet p = new OptionSet()
            {
                { "parser=",
                    "Choose underlying parser {PARSER}.",
                    (string s) => opts.Parser = s },
                { "states=",
                    "Generate a state file based on the chosen parser.",
                    (string s) => opts.StateFile = s },
                { "group-dot=",
                    "Generate a dot file from the calculated group graph.",
                    (string s) => opts.GroupDotFile = s },
                { "breakdown=",
                    "Generate a general breakdown text file.",
                    (string s) => opts.BreakdownFile = s },
                { "h|help",
                    "Show this message and exit.",
                    v => opts.ShowHelp = v != null }
            };

            List<string> input;
            try
            {
                input = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `ParMaLy --help' for more information.");
                return;
            }

            if(opts.ShowHelp)
            {
                ShowHelp(p);
                return;
            }
            
            if (input.Count != 1)
            {
                Console.WriteLine("No grammar file given.");
                Console.WriteLine("Try `ParMaLy --help' for more information.");
                return;
            }

            string file = input[0];
            Environment env = new Environment(new Logger());
            try
            {
                string source = File.ReadAllText(file);
                env.Parse(source);

            }
            catch(Error err)
            {
                Console.Error.Write(err.Type.ToString());
                return;
            }
            catch(FileNotFoundException ex)
            {
                Console.Error.Write(ex.Message);
                return;
            }

            if (!String.IsNullOrEmpty(opts.BreakdownFile))
                Output.SimpleBreakdown.Print(File.CreateText(opts.BreakdownFile), env);

            if (!String.IsNullOrEmpty(opts.GroupDotFile))
                Output.DotGraph.PrintGroupGraph(File.CreateText(opts.GroupDotFile), env);

            if (!String.IsNullOrEmpty(opts.Parser))
            {
                int parser = 0;
                if (opts.Parser.ToLower() == "lr0")
                    parser = 1;
                else
                {
                    Console.WriteLine("Unknown parser selected.");
                    Console.WriteLine("Try `ParMaLy --help' for more information.");
                    return;
                }

                if (!String.IsNullOrEmpty(opts.StateFile))
                {
                    switch(parser)
                    {
                        case 1:
                            Parser.LR0 lr0 = new Parser.LR0();
                            lr0.GenerateStates(env);
                            Output.LR0.PrintStates(File.CreateText(opts.StateFile), lr0, env, false);
                            break;
                    }
                }
            }
        }
    }
}
