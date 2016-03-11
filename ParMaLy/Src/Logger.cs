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
using DataLisp;

namespace PML
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class Logger : DataLisp.Logger
    {
        int _WarningCount = 0;
        public int WarningCount { get { return _WarningCount; } }

        int _ErrorCount = 0;
        public int ErrorCount { get { return _ErrorCount; } }

        TextWriter _Writer;

        public Logger()
        {
            _Writer = File.CreateText("pml_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".log");
        }

        public Logger(string log)
        {
            _Writer = File.CreateText(log);
        }

        public override void Log(int line, int column, DataLisp.LogLevel level, string str)
        {
            Log(line, column, (LogLevel)level, str);
        }

        public void Log(int line, int column, LogLevel level, string str)
        {
            System.Console.WriteLine("[{0}]({1}) " + level.ToString() + ": " + str, line, column);
            _Writer.WriteLine("[{0}]({1}) " + level.ToString() + ": " + str, line, column);
            _Writer.Flush();

            if (level == LogLevel.Warning)
                _WarningCount++;
            else if (level == LogLevel.Error || level == LogLevel.Fatal)
                _ErrorCount++;
        }

        public override void Log(DataLisp.LogLevel level, string str)
        {
            Log((LogLevel)level, str);
        }
        public void Log(LogLevel level, string str)
        {
            Console.WriteLine(level.ToString() + ": " + str);
            _Writer.WriteLine(level.ToString() + ": " + str);
            _Writer.Flush();

            if (level == LogLevel.Warning)
                _WarningCount++;
            else if (level == LogLevel.Error || level == LogLevel.Fatal)
                _ErrorCount++;
        }
    }
}
