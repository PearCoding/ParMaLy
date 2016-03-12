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
        public string StyleFile;

        public string GroupDotFile;
        public string BreakdownFile;

        public string Parser;
        public bool Parse = false;

        public string StateFile;
        public string StateDotFile;

        public string ActionHtmlFile;
        public string GotoHtmlFile;
        public string TransitionHtmlFile;

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
            Console.WriteLine();
            Console.WriteLine("PARSER=");
            Console.WriteLine("\tlr0\t\t[LR(0)]");
            Console.WriteLine("\tslr1\t\t[SLR(1)]");
            Console.WriteLine("\tlalr1\t\t[LALR(1)]");
            Console.WriteLine("\tlr1\t\t[LR(1)]");
            Console.WriteLine("\tlr2, lr3...\t[LR(k)]");
            Console.WriteLine("\tll0\t\t[LL(0)]");
            Console.WriteLine("\tll1\t\t[LL(1)]");
            Console.WriteLine("\tll2, ll3...\t[LL(k)]");
            Console.WriteLine("\trd\t\t[Recursive-Descent]");
        }

        static int Main(string[] args)
        {
            Logger logger = new Logger();
            Options opts = new Options();
            OptionSet p = new OptionSet()
            {
                { "parser=",
                    "Choose underlying {PARSER}.",
                    (string s) => opts.Parser = s },
                { "states=",
                    "Generate a state {FILE} based on the chosen parser.",
                    (string s) => { opts.StateFile = s; opts.Parse = true; } },
                { "group-dot=",
                    "Generate a dot {FILE} from the calculated group graph.",
                    (string s) => opts.GroupDotFile = s },
                { "state-dot=",
                    "Generate a dot {FILE} from the calculated state graph.",
                    (string s) => { opts.StateDotFile = s; opts.Parse = true; } },
                { "action-html=",
                    "Generate a html {FILE} from the calculated ACTION table.",
                    (string s) => { opts.ActionHtmlFile = s; opts.Parse = true; } },
                { "goto-html=",
                    "Generate a html {FILE} from the calculated GOTO table.",
                    (string s) => { opts.GotoHtmlFile = s; opts.Parse = true; } },
                { "transition-html=",
                    "Generate a html {FILE} from the calculated ACTION and GOTO table.",
                    (string s) => { opts.TransitionHtmlFile = s; opts.Parse = true; } },
                { "breakdown=",
                    "Generate a general breakdown text {FILE}.",
                    (string s) => opts.BreakdownFile = s },
                { "style=",
                    "The underlying style {FILE}.",
                    (string s) => opts.StyleFile = s },
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
                Console.WriteLine("Try 'ParMaLy --help' for more information.");
                return -4;
            }

            if(opts.ShowHelp)
            {
                ShowHelp(p);
                return 0;
            }
            
            if (input.Count != 1)
            {
                Console.WriteLine("No grammar file given.");
                Console.WriteLine("Try 'ParMaLy --help' for more information.");
                return -3;
            }

            string file = input[0];
            Environment env = new Environment(logger);
            try
            {
                string source = File.ReadAllText(file);
                env.Parse(source);
            }
            catch(Error err)
            {
                Console.Error.Write(err.Type.ToString());
                return 2;
            }
            catch(FileNotFoundException ex)
            {
                Console.Error.Write(ex.Message);
                return -2;
            }

            FirstSet.Setup(env);
            FollowSet.Setup(env);

            Style.Style style = null;
            if (opts.StyleFile == null)
                style = new Style.Style();
            else
                style = Style.StyleParser.Parse(File.ReadAllText(opts.StyleFile), logger);

            if (!String.IsNullOrEmpty(opts.BreakdownFile))
                Output.SimpleBreakdown.Print(File.CreateText(opts.BreakdownFile), env);

            if (!String.IsNullOrEmpty(opts.GroupDotFile))
                Output.DotGraph.PrintGroupGraph(File.CreateText(opts.GroupDotFile), env, style.GroupDot);

            if (!String.IsNullOrEmpty(opts.Parser))
            {
                Parser.BTParser parser;
                if (opts.Parser.ToLower() == "lr0")
                    parser = new Parser.LR0();
                else if (opts.Parser.ToLower() == "slr1")
                    parser = new Parser.SLR1();
                else if (opts.Parser.ToLower() == "lr1")
                    parser = new Parser.LR1();
                else
                {
                    Console.WriteLine("Unknown parser selected.");
                    Console.WriteLine("Try 'ParMaLy --help' for more information.");
                    return -1;
                }

                if (opts.Parse && parser != null)
                {
                    parser.GenerateStates(env, logger);
                    parser.GenerateActionTable(env, logger);
                    parser.GenerateGotoTable(env, logger);

                    if (!String.IsNullOrEmpty(opts.StateFile))
                    {
                        Output.StateFile.PrintStates(File.CreateText(opts.StateFile),
                                    parser.States, env, false);
                    }

                    if (!String.IsNullOrEmpty(opts.StateDotFile))
                    {
                        Output.DotGraph.PrintStateGraph(File.CreateText(opts.StateDotFile),
                            env, parser.StartState, style.StateDot);
                    }

                    if (!String.IsNullOrEmpty(opts.ActionHtmlFile))
                    {
                        Output.HtmlTable.PrintActionTable(File.CreateText(opts.ActionHtmlFile), parser.States,
                            parser.ActionTable, env, style.ActionTableHtml);
                    }

                    if (!String.IsNullOrEmpty(opts.GotoHtmlFile))
                    {
                        Output.HtmlTable.PrintGotoTable(File.CreateText(opts.GotoHtmlFile), parser.States,
                            parser.GotoTable, env, style.GotoTableHtml);
                    }

                    if (!String.IsNullOrEmpty(opts.TransitionHtmlFile))
                    {
                        Output.HtmlTable.PrintTransitionTable(File.CreateText(opts.TransitionHtmlFile), parser.States,
                            parser.ActionTable, parser.GotoTable, env, style.TransitionTableHtml);
                    }
                }
            }

            return logger.ErrorCount != 0 ? 1 : 0;
        }
    }
}
