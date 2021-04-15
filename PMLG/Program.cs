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

using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace PML
{
    class Options
    {
        public string StyleFile;

        public string Parser;
        public bool Parse = false;

        public string StateFile;
        public string StateDotFile;

        public string ActionHtmlFile;
        public string GotoHtmlFile;
        public string TransitionHtmlFile;
        public string LookupHtmlFile;

        public string CodeType;
        public string CodeFile = "a.txt";

        public string CodeLanguage = "cpp";

        public string ProceedingCSVFile;

        public string ErrorFile;

        public string PMLFile;

        public int K = 1;
        public int D = 0;

        public bool ShowHelp = false;
        public bool NoLog = false;

        public bool OmitCustomCode = false;
        public bool EnableTraceCode = false;
    }

    class Program
    {
        private static readonly Dictionary<string, Dictionary<string, Output.CodeGenerator.ICodeGenerator>> _CodeGenerators = new Dictionary<string, Dictionary<string, Output.CodeGenerator.ICodeGenerator>>
        {
            {"rd", new Dictionary<string, Output.CodeGenerator.ICodeGenerator>{
                {"cpp", new Output.CodeGenerator.RD_CPP()}}},
            {"t_lr", new Dictionary<string, Output.CodeGenerator.ICodeGenerator>{
                {"cpp", new Output.CodeGenerator.TableLR1_CPP()}}},
        };

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: PMLG grammar_file --parser=[PARSER] [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("PARSER=");
            Console.WriteLine("\tlr0\t\t[LR(0)](LR)");
            Console.WriteLine("\tlr1\t\t[LR(1)](LR)");
            Console.WriteLine("\tlr\t[LR(k)](LR)");
            Console.WriteLine("\tslr1\t\t[SLR(1, 0)](LR)");
            Console.WriteLine("\tslr\t\t[SLR(k, d)](LR)");
            Console.WriteLine("\tlalr1\t\t[LALR(1)](LR)");
            Console.WriteLine("\tlalr\t\t[LALR(k)](LR)");
            Console.WriteLine("\tll0\t\t[LL(0)](LL)");
            Console.WriteLine("\tll1\t\t[LL(1)](LL)");
            Console.WriteLine("\tll\t\t[LL(k)](LL)");
            Console.WriteLine("\trd\t\t[Recursive-Descent](Recursive)");
            Console.WriteLine("CODE TYPE=");
            Console.WriteLine("\trd\t\t[Recursive-Descent](rd)");
            Console.WriteLine("\tt_lr\t\t[Table LR](lr0,lr1,lr,slr1,slr,lalr1,lalr)");
            Console.WriteLine("\tr_lr\t\t[Recursive LR](lr0,lr1,lr,slr1,slr,lalr1,lalr)");
            Console.WriteLine("CODE LANGUAGE=");
            Console.WriteLine("\tcpp\t\t[C++])");
        }

        static int Main(string[] args)
        {
            Options opts = new Options();
            OptionSet p = new OptionSet()
            {
                { "parser=",
                    "Choose underlying {PARSER}.",
                    (string s) => opts.Parser = s },
                { "states=",
                    "Generate a state {FILE} based on the chosen parser. Only usable with LR parsers.",
                    (string s) => { opts.StateFile = s; opts.Parse = true; } },
                { "state-dot=",
                    "Generate a dot {FILE} from the calculated state graph. Only usable with LR parsers.",
                    (string s) => { opts.StateDotFile = s; opts.Parse = true; } },
                { "action-html=",
                    "Generate a html {FILE} from the calculated ACTION table. Only usable with LR parsers.",
                    (string s) => { opts.ActionHtmlFile = s; opts.Parse = true; } },
                { "goto-html=",
                    "Generate a html {FILE} from the calculated GOTO table. Only usable with LR parsers.",
                    (string s) => { opts.GotoHtmlFile = s; opts.Parse = true; } },
                { "transition-html=",
                    "Generate a html {FILE} from the calculated ACTION and GOTO table. Only usable with LR parsers.",
                    (string s) => { opts.TransitionHtmlFile = s; opts.Parse = true; } },
                { "lookup-html=",
                    "Generate a html {FILE} from the calculated LOOKUP table. Only usable with LL parsers.",
                    (string s) => { opts.LookupHtmlFile = s; opts.Parse = true; } },
                { "proceeding-csv=",
                    "Generate a csv {FILE} from generation process.",
                    (string s) => { opts.ProceedingCSVFile = s; opts.Parse = true; } },
                { "code-type=",
                    "Generate code based on {CODE TYPE}.",
                    (string s) => { opts.CodeType = s; opts.Parse = true; } },
                { "code-file=",
                    "Generate a pseudo code {FILE}.",
                    (string s) => { opts.CodeFile = s; opts.Parse = true; } },
                { "code-language=",
                    "Choose underlying {CODE LANGUAGE}.",
                    (string s) => opts.CodeLanguage = s },
                { "disable-custom-code",
                    "Disable user given code inside generated code.",
                     v => opts.OmitCustomCode = v != null },
                { "enable-trace-code",
                    "Enable debug trace code for the parser.",
                     v => opts.EnableTraceCode = v != null },
                { "errors=",
                    "Generate a error {FILE} listing all errors.",
                    (string s) => { opts.ErrorFile = s; opts.Parse = true; } },
                { "style=",
                    "The underlying style {FILE}.",
                    (string s) => opts.StyleFile = s },
                { "pml=",
                    "Generate a {FILE} to be used by PMLR.",
                    (string s) => { opts.PMLFile = s; opts.Parse = true; } },
                { "k|lookahead=",
                    "The used {MAX} amount of lookahead. Default is " + opts.K + ". Not every parser will consider this option.",
                    (int k) => opts.K = k },
                { "d=",
                    "Some parser types need another parameter. Default is " + opts.D + ".",
                    (int d) => opts.D = d },
                { "h|help", "Show this message and exit.",
                    v => opts.ShowHelp = v != null },
                { "nl|no-log", "Do not produce any log file.",
                    v => opts.NoLog = true }
            };

            List<string> input;
            try
            {
                input = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'PMLG --help' for more information.");
                return -4;
            }

            if (opts.ShowHelp)
            {
                ShowHelp(p);
                return 0;
            }

            if (input.Count != 1)
            {
                Console.WriteLine("No grammar file given.");
                Console.WriteLine("Try 'PMLG --help' for more information.");
                return -3;
            }

            string file = input[0];
            Logger logger = new Logger(!opts.NoLog);
            Environment env = new Environment(logger);
            try
            {
                string source = File.ReadAllText(file);
                env.Parse(source);
            }
            catch (Grammar.ParserError err)
            {
                Console.Error.Write(err.Type.ToString());
                return 2;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.Write(ex.Message);
                return -2;
            }

            Style.Style style = null;
            if (opts.StyleFile == null)
                style = new Style.Style();
            else
                style = Style.StyleParser.Parse(File.ReadAllText(opts.StyleFile), logger);

            Parser.IParser parser = null;
            if (!String.IsNullOrEmpty(opts.Parser))
            {
                int type = 0;
                //LR
                if (opts.Parser.ToLower() == "lr0")
                    parser = new Parser.LR0();
                else if (opts.Parser.ToLower() == "lr1")
                    parser = new Parser.LR(1);
                else if (opts.Parser.ToLower() == "lr")
                {
                    parser = (opts.K < 1 ? (Parser.IParser)new Parser.LR0() : new Parser.LR(opts.K));
                }
                //SLR
                else if (opts.Parser.ToLower() == "slr1")
                    parser = new Parser.SLR();
                else if (opts.Parser.ToLower() == "slr")
                    parser = new Parser.SLR(opts.K < 1 ? 1 : opts.K, opts.D < 0 ? 0 : opts.D);
                //LARL
                else if (opts.Parser.ToLower() == "lalr1")
                    parser = new Parser.LALR(1);
                else if (opts.Parser.ToLower() == "lalr")
                    parser = new Parser.LALR(opts.K < 1 ? 1 : opts.K);
                //LL
                else if (opts.Parser.ToLower() == "ll0")
                {
                    parser = new Parser.LL0();
                    type = 1;
                }
                else if (opts.Parser.ToLower() == "ll1")
                {
                    parser = new Parser.LLK(1);
                    type = 1;
                }
                else if (opts.Parser.ToLower() == "ll")
                {
                    if (opts.K <= 0)
                        parser = new Parser.LL0();
                    else
                        parser = new Parser.LLK(opts.K);

                    type = 1;
                }
                //RD
                else if (opts.Parser.ToLower() == "rd")
                {
                    parser = new Parser.RD(opts.K < 1 ? 1 : opts.K);
                    type = 2;
                }
                else
                {
                    Console.WriteLine("Unknown parser selected.");
                    Console.WriteLine("Try 'PMLA --help' for more information.");
                    return -1;
                }

                if (opts.Parse && parser != null)
                {
                    parser.Generate(env, logger);

                    if (type == 0)
                    {
                        Parser.IBUParser buParser = parser as Parser.IBUParser;

                        if (!String.IsNullOrEmpty(opts.StateFile))
                        {
                            Output.StateFile.PrintStates(File.CreateText(opts.StateFile),
                                        buParser.States, env, false);
                        }

                        if (!String.IsNullOrEmpty(opts.StateDotFile))
                        {
                            Output.DotGraph.PrintStateGraph(File.CreateText(opts.StateDotFile),
                                env, buParser.StartState, style.StateDot);
                        }

                        if (!String.IsNullOrEmpty(opts.ActionHtmlFile))
                        {
                            Output.HtmlTable.PrintActionTable(File.CreateText(opts.ActionHtmlFile),
                                buParser.ActionTable, env, style.ActionTableHtml);
                        }

                        if (!String.IsNullOrEmpty(opts.GotoHtmlFile))
                        {
                            Output.HtmlTable.PrintGotoTable(File.CreateText(opts.GotoHtmlFile),
                                buParser.GotoTable, env, style.GotoTableHtml);
                        }

                        if (!String.IsNullOrEmpty(opts.TransitionHtmlFile))
                        {
                            Output.HtmlTable.PrintTransitionTable(File.CreateText(opts.TransitionHtmlFile),
                                buParser.ActionTable, buParser.GotoTable, env, style.TransitionTableHtml);
                        }

                        if (!String.IsNullOrEmpty(opts.ProceedingCSVFile))
                        {
                            Output.CSV.PrintProceedings(File.CreateText(opts.ProceedingCSVFile), parser.Statistics.BU.Proceedings,
                                style.ProceedingCSV);
                        }

                        if (!String.IsNullOrEmpty(opts.PMLFile))
                        {
                            Output.PMLWriter.WriteLR(File.CreateText(opts.PMLFile), ((Parser.IBUParser)parser), env);
                        }
                    }
                    else if (type == 1)//LLParser
                    {
                        if (!String.IsNullOrEmpty(opts.LookupHtmlFile))
                        {
                            Output.HtmlTable.PrintLookupTable(File.CreateText(opts.LookupHtmlFile), ((Parser.ITDParser)parser).Lookup, env, style.LookupTableHtml);
                        }

                        if (!String.IsNullOrEmpty(opts.PMLFile))
                        {
                            Output.PMLWriter.WriteLL(File.CreateText(opts.PMLFile), ((Parser.ITDParser)parser), env);
                        }
                    }

                    if (!String.IsNullOrEmpty(opts.CodeType))
                    {
                        if (_CodeGenerators.ContainsKey(opts.CodeType))
                        {
                            if (_CodeGenerators[opts.CodeType].ContainsKey(opts.CodeLanguage))
                            {
                                Output.CodeGenerator.ICodeGenerator gen = _CodeGenerators[opts.CodeType][opts.CodeLanguage];
                                Output.CodeGenerator.CodeGeneratorSettings settings = new Output.CodeGenerator.CodeGeneratorSettings();
                                settings.EmbedCustomCode = !opts.OmitCustomCode;
                                settings.EmbedTraceCode = opts.EnableTraceCode;

                                gen.Generate(File.CreateText(opts.CodeFile), parser, env, style.Code, settings);
                            }
                            else
                            {
                                Console.WriteLine("Unknown code language selected.");
                                Console.WriteLine("Try 'PMLA --help' for more information.");
                                return -1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unknown code type selected.");
                            Console.WriteLine("Try 'PMLA --help' for more information.");
                            return -1;
                        }
                    }

                    if (!String.IsNullOrEmpty(opts.ErrorFile))
                    {
                        Output.ErrorFile.Print(File.CreateText(opts.ErrorFile), parser.Statistics);
                    }
                }
            }

            return logger.ErrorCount;
        }
    }
}
