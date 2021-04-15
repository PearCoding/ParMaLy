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
using System.Collections.Generic;
using System.IO;

namespace PML
{
    class Options
    {
        public string EventFile;
        public bool ShowHelp = false;
        public bool NoLog = false;
    }

    class Program
    {
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: PMLR pml_file grammar_file input_file [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static int Main(string[] args)
        {
            Options opts = new Options();
            OptionSet p = new OptionSet()
            {
                { "events=", "Print out all events to {FILE}.",
                    (string v) => opts.EventFile = v },
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
                Console.WriteLine("Try 'PMLR --help' for more information.");
                return -1;
            }

            if (opts.ShowHelp)
            {
                ShowHelp(p);
                return 0;
            }

            if (input.Count != 3)
            {
                Console.WriteLine("Not enough input given.");
                Console.WriteLine("Try 'PMLR --help' for more information.");
                return -2;
            }

            Logger logger = new Logger(!opts.NoLog);
            Environment env = new Environment(logger);
            Runner.IRunner runner = null;
            TokenLexer lexer = new TokenLexer();

            try
            {
                string source = File.ReadAllText(input[1]);
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

            try
            {
                runner = PMLReader.Read(File.OpenText(input[0]), env, logger);
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.Write(ex.Message);
                return -2;
            }

            try
            {
                lexer.Parse(File.ReadAllText(input[2]));
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.Write(ex.Message);
                return -2;
            }

            if (runner == null || lexer == null)
                return -3;

            IEnumerable<Events.IEvent> events = runner.Run(lexer, env, logger);

            if (!String.IsNullOrEmpty(opts.EventFile))
            {
                Output.EventFile.PrintEvents(File.CreateText(opts.EventFile), events);
            }

            return logger.ErrorCount;
        }
    }
}
