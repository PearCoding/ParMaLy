/*
 * Copyright (c) 2016-2019, Ömercan Yazici <omercan AT pearcoding.eu>
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

namespace PML.Output.CodeGenerator
{
    using R;

    public class CPPEmbeddedCodeGenerator
    {
        private char _P_IDENT = '$';
        private string _ReturnValue;
        private string _ParamPrefix;

        public CPPEmbeddedCodeGenerator(string retVal, string paramPrefix)
        {
            _ReturnValue = retVal;
            _ParamPrefix = paramPrefix;
        }

        public string Generate(Rule rule)
        {
            if (string.IsNullOrEmpty(rule.Code))
                return "";

            Dictionary<string, string> paramMapper = new Dictionary<string, string>();
            int counter = 0;
            foreach (var t in rule.Tokens)
            {
                if (!string.IsNullOrEmpty(t.ReturnType))
                {
                    if (!string.IsNullOrEmpty(t.CodeIdentifier))
                    {
                        paramMapper[t.CodeIdentifier] = _ParamPrefix + t.CodeIdentifier;
                    }

                    paramMapper[counter.ToString()] = _ParamPrefix + counter;
                }
                counter++;
            }

            string oldCode = rule.Code.Trim();
            string newCode = "";
            for (int i = 0; i < oldCode.Length;)
            {
                if (oldCode[i] == '\\' && i + 1 < oldCode.Length && oldCode[i + 1] == _P_IDENT)
                {
                    i += 2;
                    newCode += _P_IDENT;
                }
                else if (oldCode[i] == _P_IDENT && i + 1 < oldCode.Length)
                {
                    i += 1;
                    if (oldCode[i] == _P_IDENT)// Return Value
                    {
                        newCode += _ReturnValue;
                        ++i;
                    }
                    else
                    {
                        string paramName = "";
                        for (; i < oldCode.Length; ++i)
                        {
                            if (char.IsLetterOrDigit(oldCode[i]) || oldCode[i] == '_')
                            {
                                paramName += oldCode[i];
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!paramMapper.ContainsKey(paramName))
                        {
                            newCode += "/* UNKNOWN " + paramName + " */";
                        }
                        else
                        {
                            newCode += paramMapper[paramName];
                        }
                    }
                }
                else
                {
                    newCode += oldCode[i];
                    ++i;
                }
            }

            return newCode;
        }
    }
}
