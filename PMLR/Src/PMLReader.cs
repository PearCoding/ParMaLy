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

using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PML
{
    public static class PMLReader
    {
        public static Runner.IRunner Read(TextReader reader, Environment env, Logger logger)
        {
            var header = reader.ReadLine().Split(' ');
            if(header.Length != 3)
            {
                logger.Log(1, 1, LogLevel.Error, "Invalid header");
                return null;
            }

            if(header[0] != "PML")
            {
                logger.Log(1, 1, LogLevel.Error, "Invalid magic");
                return null;
            }

            int k = int.Parse(header[2]);

            if (header[1] == "LL")
            {
                return ReadLL(reader, k, env, logger);
            }
            else if (header[1] == "LR")
            {
                return ReadLR(reader, k, env, logger);
            }
            else
            {
                logger.Log(1, 4, LogLevel.Error, "Unknown parser type");
                return null;
            }
        }

        // LL
        public static Runner.LLRunner ReadLL(TextReader reader, int k, Environment env, Logger logger)
        {
            var lookaheads = ReadLookahead(reader, env);

            Runner.LLRunner runner = new Runner.LLRunner(k);
            foreach(var grp in env.Groups)
            {
                var line = reader.ReadLine().Trim().Split(' ');

                if(line[0] == grp.ID.ToString())
                {
                    for(int i = 1; i < line.Length; ++i)
                    {
                        if (line[i] != "-")
                            runner.Table.Set(grp, lookaheads[i - 1], env.RuleByID(int.Parse(line[i])));
                    }
                }
                else
                {
                    logger.Log(LogLevel.Error, "Invalid PML");
                    return null;
                }
            }

            return runner;
        }

        // LR
        public static Runner.LRRunner ReadLR(TextReader reader, int k, Environment env, Logger logger)
        {
            var lookaheads = ReadLookahead(reader, env);
            Runner.LRRunner runner = new Runner.LRRunner(k);
            int stateCount = int.Parse(reader.ReadLine());
            
            for(int state = 0; state < stateCount; ++state)//Action
            {
                var line = reader.ReadLine().Trim().Split(' ');

                if (line[0] == state.ToString())
                {
                    for (int i = 1; i < line.Length; ++i)
                    {
                        if (line[i] != "-")
                        {
                            if (line[i] == "a")
                                runner.ActionTable.Set(state, lookaheads[i - 1], BU.ActionTable.Action.Accept, -1);
                            else if (line[i].First() == 's')
                                runner.ActionTable.Set(state, lookaheads[i - 1], BU.ActionTable.Action.Shift,
                                    int.Parse(line[i].Substring(1)));
                            else if (line[i].First() == 'r')
                                runner.ActionTable.Set(state, lookaheads[i - 1], BU.ActionTable.Action.Reduce,
                                    int.Parse(line[i].Substring(1)));
                            else
                            {
                                logger.Log(LogLevel.Error, "Invalid PML");
                                return null;
                            }
                        }
                    }
                }
                else
                {
                    logger.Log(LogLevel.Error, "Invalid PML");
                    return null;
                }
            }
            
            reader.ReadLine();

            for (int state = 0; state < stateCount; ++state)//Goto
            {
                var line = reader.ReadLine().Trim().Split(' ');

                if (line[0] == state.ToString())
                {
                    for (int i = 1; i < line.Length; ++i)
                    {
                        if (line[i] != "-")
                            runner.GotoTable.Set(state, env.Groups[i - 1], int.Parse(line[i]));
                    }
                }
                else
                {
                    logger.Log(LogLevel.Error, "Invalid PML");
                    return null;
                }
            }

            return runner;
        }

        // Utils
        static List<RuleLookahead> ReadLookahead(TextReader reader, Environment env)
        {
            List<RuleLookahead> list = new List<RuleLookahead>();

            var line = reader.ReadLine();

            foreach(var s in line.Split(' '))
            {
                if(s == "-")
                {
                    if(!list.Contains(null))
                        list.Add(null);
                }
                else
                {
                    var n = s.Remove(s.Length - 1, 1).Remove(0, 1);

                    RuleLookahead lookahead = new RuleLookahead();
                    foreach (var t in n.Split(','))
                    {
                        int i = int.Parse(t);
                        lookahead.Add(env.TokenByID(i));
                    }

                    if (!list.Contains(lookahead))
                        list.Add(lookahead);
                }
            }

            return list;
        }
    }
}
