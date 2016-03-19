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

        public string FirstSetFile;
        public string FollowSetFile;
        public string PredictSetFile;

        public string OutputFile;

        public bool FixLeftRecursion = false;

        public int K = 1;

        public bool ShowHelp = false;
        public bool NoLog = false;
    }

    class Program
    {
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: PMLA grammar_file [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static int Main(string[] args)
        {
            Options opts = new Options();
            OptionSet p = new OptionSet()
            {
                { "group-dot=",
                    "Generate a dot {FILE} from the calculated group graph.",
                    (string s) => opts.GroupDotFile = s },
                { "breakdown=",
                    "Generate a general breakdown text {FILE}.",
                    (string s) => opts.BreakdownFile = s },
                { "style=",
                    "The underlying style {FILE}.",
                    (string s) => opts.StyleFile = s },
                { "firstset=",
                    "Generate a {FILE} with all entries in the first sets. Makes use of the -k option.",
                    (string s) => { opts.FirstSetFile = s; } },
                { "followset=",
                    "Generate a {FILE} with all entries in the follow sets. Makes use of the -k option.",
                    (string s) => { opts.FollowSetFile = s; } },
                { "predictset=",
                    "Generate a {FILE} with all entries in the predict sets. Makes use of the -k option.",
                    (string s) => { opts.PredictSetFile = s; } },
                { "o|output=",
                    "Print out preprocessed grammar file into {FILE}.",
                    (string s) => { opts.OutputFile = s; } },
                { "left-recursion",
                    "Fix left recursion.",
                    v => opts.FixLeftRecursion = true },
                { "k|lookahead=",
                    "The used {MAX} amount of lookahead. Default is " + opts.K + ". Not every parser will consider this option.",
                    (int k) => opts.K = k },
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
                Console.WriteLine("Try 'PMLA --help' for more information.");
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
                Console.WriteLine("Try 'PMLA --help' for more information.");
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
            catch(Grammar.ParserError err)
            {
                Console.Error.Write(err.Type.ToString());
                return 2;
            }
            catch(FileNotFoundException ex)
            {
                Console.Error.Write(ex.Message);
                return -2;
            }

            // Preprocess stuff:
            if (opts.FixLeftRecursion)
                env = Preprocess.LeftRecursion.FixLeftRecursion(env);

            // Analyze stuff:
            Style.Style style = null;
            if (opts.StyleFile == null)
                style = new Style.Style();
            else
                style = Style.StyleParser.Parse(File.ReadAllText(opts.StyleFile), logger);

            if (!String.IsNullOrEmpty(opts.GroupDotFile))
                Output.DotGraph.PrintGroupGraph(File.CreateText(opts.GroupDotFile), env, style.GroupDot);

            if (!String.IsNullOrEmpty(opts.FirstSetFile))
            {
                for (int i = 1; i <= (opts.K < 1 ? 1 : opts.K); ++i)
                {
                    env.FirstCache.Setup(env, i);
                }

                Output.SetFile.PrintFirstSets(File.CreateText(opts.FirstSetFile), env.FirstCache, env);
            }

            if (!String.IsNullOrEmpty(opts.FollowSetFile))
            {
                for (int i = 1; i <= (opts.K < 1 ? 1 : opts.K); ++i)
                {
                    env.FirstCache.Setup(env, i);//We can't assume that first sets are already created. The cache will not do it twice ;)
                    env.FollowCache.Setup(env, i);
                }

                Output.SetFile.PrintFollowSets(File.CreateText(opts.FollowSetFile), env.FollowCache, env);
            }

            if (!String.IsNullOrEmpty(opts.PredictSetFile))
            {
                for (int i = 1; i <= (opts.K < 1 ? 1 : opts.K); ++i)
                {
                    env.FirstCache.Setup(env, i);//We can't assume that first sets are already created. The cache will not do it twice ;)
                    env.FollowCache.Setup(env, i);
                }

                Output.SetFile.PrintPredictSets(File.CreateText(opts.PredictSetFile), env);
            }

            if (!String.IsNullOrEmpty(opts.BreakdownFile))
                Output.SimpleBreakdown.Print(File.CreateText(opts.BreakdownFile), env);

            if (!String.IsNullOrEmpty(opts.OutputFile))
                Output.Grammar.PrintGrammar(File.CreateText(opts.OutputFile), env);

            return logger.ErrorCount;
        }
    }
}
