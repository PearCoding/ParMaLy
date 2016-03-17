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

namespace PML
{
    class TokenLexer
    {
        List<string> _Input = new List<string>();
        int _Position = 0;

        public void Parse(string source)
        {
            _Input.Clear();
            _Position = 0;

            string tmp = "";
            bool isString = false;
            bool isEscape = false;
            foreach(var c in source)
            {
                if(!isEscape && c == '\\')
                {
                    isEscape = true;
                }
                else
                {
                    if(!isEscape && c == '"')
                    {
                        if (isString)
                        {
                            isString = false;
                            if (tmp != "")
                            {
                                _Input.Add(tmp);
                                tmp = "";
                            }
                        }
                        else
                        {
                            isString = true;
                        }
                    }
                    else if(c == '\t' || c == '\n' || c == '\r' || c == ' ')
                    {
                        if(tmp != "")
                        {
                            _Input.Add(tmp);
                            tmp = "";
                        }
                    }
                    else
                    {
                        tmp += c;
                    }
                    isEscape = false;
                }
            }

            if (tmp != "")
            {
                _Input.Add(tmp);
            }
        }

        public void Reset()
        {
            _Position = 0;
        }

        public int Left { get { return _Input.Count - _Position - 1; } }

        public bool IsValid(int lookahead = 0)
        {
            return Left >= (lookahead + 1);
        }

        public RuleToken Current(Environment env, int lookahead = 0)
        {
            string str = _Input[_Position + lookahead];
            foreach(var t in env.Tokens)
            {
                if (t.Name == str)
                    return t;
            }

            return null;
        }

        public void Step(int lookahead = 0)
        {
            _Position += lookahead;
        }
    }
}
