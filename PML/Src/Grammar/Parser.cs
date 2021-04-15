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

namespace PML.Grammar
{
    internal class Parser
    {
        readonly Lexer _Lexer;
        readonly Logger _Logger;

        public Parser(string source, Logger logger)
        {
            _Lexer = new Lexer(source, logger);
            _Logger = logger;
        }

        public SyntaxTree Parse()
        {
            return gr_tr_unit();
        }

        public List<RuleDefToken> ParseLine()
        {
            return gr_rule_list_line();
        }

        // Internal
        SyntaxTree gr_tr_unit()
        {
            SyntaxTree tree = new SyntaxTree();
            tree.Statements = gr_stmt_list();
            return tree;
        }

        List<Statement> gr_stmt_list()
        {
            List<Statement> list = new List<Statement>();
            while (!IsEOF())
            {
                list.Add(gr_stmt());
            };

            return list;
        }

        Statement gr_stmt()
        {
            if (Lookahead(TokenType.Token))
                return gr_token_stmt();
            else if (Lookahead(TokenType.Start))
                return gr_start_stmt();
            else
                return gr_rule_stmt();
        }

        Statement gr_token_stmt()
        {
            Match(TokenType.Token);
            string returnType = gr_return_type_specifier();
            Token token = Match(TokenType.Identifier);
            Match(TokenType.Semicolon);

            return new TokenDefStatement(token.Value, returnType);
        }

        Statement gr_start_stmt()
        {
            Match(TokenType.Start);
            Token token = Match(TokenType.Identifier);
            Match(TokenType.Semicolon);

            return new StartDefStatement(token.Value);
        }

        Statement gr_rule_stmt()
        {
            Token name = Match(TokenType.Identifier);
            string returnType = gr_return_type_specifier();
            Match(TokenType.Colon);

            RuleStatement stmt = new RuleStatement(returnType);
            while (!IsEOF())
            {
                RuleDef rule = new RuleDef(name.Value);
                rule.Tokens = gr_rule_list();
                if (Lookahead(TokenType.Code))
                    rule.Code = Match(TokenType.Code).Value;

                stmt.Rules.Add(rule);
                if (!Lookahead(TokenType.Semicolon))
                {
                    Match(TokenType.Bar);
                }
                else
                    break;
            }
            Match(TokenType.Semicolon);

            return stmt;
        }

        List<RuleDefToken> gr_rule_list()
        {
            List<RuleDefToken> rules = new List<RuleDefToken>();

            while (!Lookahead(TokenType.Bar) && !Lookahead(TokenType.Semicolon) && !Lookahead(TokenType.Code) && !IsEOF())
            {
                RuleDefToken t = gr_rule_part();
                rules.Add(t);
            }

            return rules;
        }

        List<RuleDefToken> gr_rule_list_line()
        {
            List<RuleDefToken> rules = new List<RuleDefToken>();

            while (!IsEOF())
            {
                RuleDefToken t = gr_rule_part();
                rules.Add(t);
            }

            return rules;
        }

        RuleDefToken gr_rule_part()
        {
            RuleDefToken token;
            if (Lookahead(TokenType.String))
                token = new RuleDefToken(Match(TokenType.String).Value, true);
            else
                token = new RuleDefToken(Match(TokenType.Identifier).Value, false);

            if (Lookahead(TokenType.OpenBracket))
            {
                Match(TokenType.OpenBracket);
                token.CodeIdentifier = Match(TokenType.Identifier).Value;
                Match(TokenType.CloseBracket);
            }
            return token;
        }

        string gr_return_type_specifier()
        {
            if (Lookahead(TokenType.OpenBrokets))
            {
                Match(TokenType.OpenBrokets);
                string ret = Match(TokenType.Identifier).Value;
                Match(TokenType.CloseBrokets);
                return ret;
            }
            else
            {
                return "";
            }
        }

        // Utils
        Token Match(TokenType type)
        {
            Token token = _Lexer.Next();
            if (token.Type != type)
            {
                throw new ParserError(ErrorType.Parser_WrongToken, _Lexer.CurrentLine, _Lexer.CurrentColumn, type, token.Type);
            }
            return token;
        }

        bool Lookahead(TokenType type)
        {
            Token token = _Lexer.Look();
            return token.Type == type;
        }

        bool IsEOF()
        {
            return _Lexer.Look().Type == TokenType.EOF;
        }
    }
}
