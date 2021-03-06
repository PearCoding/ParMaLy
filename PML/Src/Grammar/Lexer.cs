﻿/*
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

namespace PML.Grammar
{
    internal class Lexer
    {
        readonly string _Source;
        readonly Logger _Logger;
        int _Position = 0;

        int _Line = 1;
        public int CurrentLine
        {
            get { return _Line; }
        }

        int _Column = 1;
        public int CurrentColumn
        {
            get { return _Column; }
        }

        public Lexer(string source, Logger logger)
        {
            _Source = source;
            _Logger = logger;
        }

        public Token Next()
        {
            if (_Position >= _Source.Length)
            {
                return new Token(TokenType.EOF);
            }

            if (CurrentChar == '#')// Comment
            {
                while (_Position < _Source.Length && CurrentChar != '\n')
                {
                    ++_Position;
                    ++_Column;
                }

                return Next();
            }
            else if (CurrentChar == '/')// Other comments
            {
                ++_Position;
                ++_Column;

                if (_Position < _Source.Length && CurrentChar == '/')
                {
                    while (_Position < _Source.Length && CurrentChar != '\n')
                    {
                        ++_Position;
                        ++_Column;
                    }

                    return Next();
                }
                else if (_Position < _Source.Length && CurrentChar == '*')
                {
                    while (_Position < _Source.Length)
                    {
                        ++_Position;
                        ++_Column;


                        if (_Position < _Source.Length && CurrentChar == '*')
                        {
                            ++_Position;
                            ++_Column;

                            if (_Position < _Source.Length && CurrentChar == '/')
                            {
                                ++_Position;
                                ++_Column;
                                break;
                            }
                        }
                    }
                    return Next();
                }
                else
                {
                    throw new ParserError(ErrorType.Lexer_InvalidComment,
                        _Column, _Line);
                }
            }
            else if (CurrentChar == '{')// Code section
            {
                ++_Position;
                ++_Column;

                string str = "";
                while (_Position < _Source.Length)
                {
                    if (CurrentChar == '\\')
                    {
                        ++_Position;
                        ++_Column;

                        if (_Position >= _Source.Length)
                            break;
                    }
                    else if (CurrentChar == '}')
                    {
                        ++_Position;
                        ++_Column;
                        break;
                    }

                    str += CurrentChar;
                    ++_Position;
                    ++_Column;
                }

                return new Token(TokenType.Code, str);
            }
            else if (CurrentChar == ':')
            {
                ++_Position;
                ++_Column;

                return new Token(TokenType.Colon);
            }
            else if (CurrentChar == ';')
            {
                ++_Position;
                ++_Column;

                return new Token(TokenType.Semicolon);
            }
            else if (CurrentChar == '|')
            {
                ++_Position;
                ++_Column;

                return new Token(TokenType.Bar);
            }
            else if (CurrentChar == '<')
            {
                ++_Position;
                ++_Column;

                return new Token(TokenType.OpenBrokets);
            }
            else if (CurrentChar == '>')
            {
                ++_Position;
                ++_Column;

                return new Token(TokenType.CloseBrokets);
            }
            else if (CurrentChar == '[')
            {
                ++_Position;
                ++_Column;

                return new Token(TokenType.OpenBracket);
            }
            else if (CurrentChar == ']')
            {
                ++_Position;
                ++_Column;

                return new Token(TokenType.CloseBracket);
            }
            else if (CurrentChar == '%')//Specials
            {
                ++_Position;
                ++_Column;

                string str = "";
                while (_Position < _Source.Length && IsAscii(CurrentChar))
                {
                    str += CurrentChar;

                    ++_Position;
                    ++_Column;
                }

                if (str == "token")
                    return new Token(TokenType.Token);
                else if (str == "start")
                    return new Token(TokenType.Start);

                return Next();
            }
            else if (CurrentChar == '"' || CurrentChar == '\'')//String
            {
                char start = CurrentChar;
                string str = "";
                while (true)
                {
                    ++_Position;
                    ++_Column;

                    if (_Position >= _Source.Length ||
                        CurrentChar == '\n')
                    {
                        throw new ParserError(ErrorType.Lexer_StringNotClosed,
                            _Column, _Line, str);
                    }
                    else if (CurrentChar == '\\')
                    {
                        ++_Position;
                        ++_Column;

                        if (_Position >= _Source.Length ||
                            CurrentChar == '\n')
                        {
                            throw new ParserError(ErrorType.Lexer_InvalidOperator, _Column, _Line,
                                "\\");
                        }
                        str += CurrentChar;
                    }
                    else if (CurrentChar == start)
                    {
                        ++_Position;
                        ++_Column;
                        break;
                    }
                    else
                    {
                        str += CurrentChar;
                    }
                }

                return new Token(TokenType.String, str);
            }
            else if (IsAlpha(CurrentChar))//Identifier
            {
                string identifier = "";
                identifier += CurrentChar;

                ++_Position;
                ++_Column;
                while (_Position < _Source.Length)
                {
                    if (IsAscii(CurrentChar))
                    {
                        identifier += CurrentChar;
                        ++_Position;
                        ++_Column;
                    }
                    else
                    {
                        break;
                    }
                }

                return new Token(TokenType.Identifier, identifier);
            }
            else if (IsWhitespace(CurrentChar))
            {
                if (CurrentChar == '\n')
                {
                    ++_Line;
                    _Column = 0;
                }

                ++_Position;
                ++_Column;
                return Next();
            }
            else
            {
                throw new ParserError(ErrorType.Lexer_UnknownCharacter, _Column, _Line, CurrentChar);
            }
        }

        public Token Look()
        {
            int pos = _Position;
            int line = _Line;
            int col = _Column;

            Token t = Next();

            _Position = pos;
            _Line = line;
            _Column = col;

            return t;
        }

        static private bool IsWhitespace(char c)
        {
            if (c == ' ' || c == '\t' || c == '\r' ||
                c == '\n' || c == '\v' || c == '\f')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static private bool IsAscii(char c)
        {
            if (IsDigit(c) || IsAlpha(c))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        static private bool IsDigit(char c)
        {
            if (c == '1' || c == '2' || c == '3' ||
            c == '4' || c == '5' || c == '6' ||
            c == '7' || c == '8' || c == '9' ||
            c == '0')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static private bool IsAlpha(char c)
        {
            if (c == 'a' || c == 'b' ||
            c == 'c' || c == 'd' || c == 'e' ||
            c == 'f' || c == 'g' || c == 'h' ||
            c == 'i' || c == 'j' || c == 'k' ||
            c == 'l' || c == 'm' || c == 'n' ||
            c == 'o' || c == 'p' || c == 'q' ||
            c == 'r' || c == 's' || c == 't' ||
            c == 'u' || c == 'v' || c == 'w' ||
            c == 'x' || c == 'y' || c == 'z' ||
            c == 'A' || c == 'B' || c == 'C' ||
            c == 'D' || c == 'E' || c == 'F' ||
            c == 'G' || c == 'H' || c == 'I' ||
            c == 'J' || c == 'K' || c == 'L' ||
            c == 'M' || c == 'N' || c == 'O' ||
            c == 'P' || c == 'Q' || c == 'R' ||
            c == 'S' || c == 'T' || c == 'U' ||
            c == 'V' || c == 'W' || c == 'X' ||
            c == 'Y' || c == 'Z' || c == '_')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static private bool IsInteger(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (!IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        static private bool IsFloat(string str)
        {
            bool point = false;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];

                if (c != '.' &&
                !IsDigit(c))
                {
                    return false;
                }
                else if (c == '.')
                {
                    if (point)
                        return false;

                    point = true;
                }
            }

            return true;
        }

        private char CurrentChar
        {
            get { return _Source[_Position]; }
        }
    }
}
