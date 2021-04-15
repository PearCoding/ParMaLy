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
using System.IO;
using System.Linq;

namespace PML.Output.CodeGenerator
{
    using PML.Parser;

    public class TableLR1_CPP : ICodeGenerator
    {
        private class TokenMapper
        {
            public Dictionary<RuleToken, string> StringMap = new Dictionary<RuleToken, string>();
            public Dictionary<RuleToken, int> NumberMap = new Dictionary<RuleToken, int>();
        }
        private class GroupMapper
        {
            public Dictionary<RuleGroup, string> StringMap = new Dictionary<RuleGroup, string>();
            public Dictionary<RuleGroup, int> NumberMap = new Dictionary<RuleGroup, int>();
        }

        private CodeGeneratorSettings _Settings;
        private readonly CPPEmbeddedCodeGenerator _EmbeddedGenerator = new CPPEmbeddedCodeGenerator("__ret", "p_");

        public void Generate(TextWriter writer, Parser.IParser parser, Environment env, Style.CodeStyle style, CodeGeneratorSettings settings)
        {

            if (!(parser is Parser.IBUParser))
            {
                return;
            }

            _Settings = settings;
            IBUParser context = ((Parser.IBUParser)parser);

            writer.WriteLine("struct ParseResult {");
            writer.WriteLine("\tbool Successful;");
            writer.WriteLine("\tvoid* Return;");
            writer.WriteLine("};");
            writer.WriteLine();
            TokenMapper token_mapper = SetupTokenTable(writer, context, env);
            writer.WriteLine("class Parser {");
            writer.WriteLine("protected:");
            writer.WriteLine("\tvirtual BUToken nextToken(void** param) = 0;");
            if (_Settings.EmbedCustomCode)
                SetupEmbeddedCodeFunctionsHead(writer, context, env);

            writer.WriteLine("public:");
            writer.WriteLine("\tParseResult parse();");
            writer.WriteLine("};");
            writer.WriteLine();

            SetupUtils(writer);
            GroupMapper group_mapper = SetupGroupTable(writer, context, env);
            SetupActionTables(writer, context, env, token_mapper);
            SetupGotoTables(writer, context, env, group_mapper);
            SetupGlueTables(writer, context, env, group_mapper);
            SetupParseLoop(writer, context, env);
            if (_Settings.EmbedCustomCode)
                SetupEmbeddedCodeFunctions(writer, context, env);

            writer.Flush();
        }

        private void SetupUtils(TextWriter writer)
        {
            writer.WriteLine("/* Utility functions */");
            writer.WriteLine("typedef uint64_t table_entry_t;");
            writer.WriteLine("typedef uint64_t state_t;");
            writer.WriteLine("enum ActionType {AT_INVALID=0, AT_ACCEPT, AT_REDUCE, AT_SHIFT};");
            writer.WriteLine("inline ActionType extractAction(table_entry_t entry) { return (ActionType)((0xC000000000000000 & entry) >> 0x3E); }");
            writer.WriteLine("inline state_t extractState(table_entry_t entry) { return (0x3FFFFFFFFFFFFFFF & entry); }");
            writer.WriteLine();
        }
        private TokenMapper SetupTokenTable(TextWriter writer, Parser.IBUParser context, Environment env)
        {
            writer.WriteLine("/* Tokens returned by the lexer */");
            writer.WriteLine("enum BUToken {");
            writer.WriteLine("\t_BUT_EOF=0,");
            TokenMapper token_mapper = new TokenMapper();
            int counter = 0;
            int defcounter = 1000;
            foreach (RuleToken t in env.Tokens)
            {
                if (t.IsComplex)
                {
                    string name = "BUT_" + t.Name;
                    writer.WriteLine("\t" + name + "=" + defcounter + ",");

                    token_mapper.StringMap[t] = name;
                    ++defcounter;
                }
                else
                {
                    string name = "BUT_" + counter;
                    if (t.Name.Length == 1 && t.Name[0] < 128)// ASCII
                    {
                        writer.WriteLine("\t" + name + "=" + (int)(t.Name[0]) + ",\t// " + t.Name);
                    }
                    else
                    {
                        writer.WriteLine("\t" + name + "=" + defcounter + ",\t// " + t.Name);
                        ++defcounter;
                    }

                    token_mapper.StringMap[t] = name;
                    ++counter;
                }
                token_mapper.NumberMap[t] = token_mapper.StringMap.Count;
            }
            writer.WriteLine("};");
            writer.WriteLine();
            return token_mapper;
        }

        private GroupMapper SetupGroupTable(TextWriter writer, Parser.IBUParser context, Environment env)
        {
            /* We use the output only for debugging reasons */
            writer.WriteLine("/* Rule groups defined by the grammar */");
            writer.WriteLine("enum BURuleGroup {");
            GroupMapper group_mapper = new GroupMapper();
            int counter = 0;
            foreach (RuleGroup t in env.Groups)
            {
                string name = "BURG_" + counter;
                writer.WriteLine("\t" + name + "=" + counter + ",\t// " + t.Name);

                group_mapper.NumberMap[t] = counter;
                group_mapper.StringMap[t] = name;
                counter++;
            }
            writer.WriteLine("};");
            writer.WriteLine();
            return group_mapper;
        }

        private void SetupActionTables(TextWriter writer, Parser.IBUParser context, Environment env, TokenMapper tokenMapper)
        {
            // TODO: Make a compressed version!
            // TODO: Make lr(k) version, not only k=1
            BU.ActionTable actionTable = context.ActionTable;
            int k = 1; //context.K

            writer.WriteLine("/* Action Table */");
            writer.WriteLine("constexpr int LookaheadCount = " + k + ";");
            writer.WriteLine("constexpr int TokenCount = " + (tokenMapper.NumberMap.Count + 1) + ";");
            writer.WriteLine("constexpr int StateCount = " + actionTable.Rows.Count() + ";");

            // Indirect Column Accessor
            int indirectTokenArraySize = (int)System.Math.Pow(tokenMapper.NumberMap.Count + 1, k);
            long[] indirectTokenMap = new long[indirectTokenArraySize];
            int counter = 0;
            foreach (RuleLookahead lookahead in actionTable.Colums)
            {
                if (lookahead == null || lookahead.Count < 1 || lookahead[0] == null) // EOF
                {
                    indirectTokenMap[0] = counter;
                }
                else
                {
                    RuleToken t = lookahead[0];
                    indirectTokenMap[tokenMapper.NumberMap[t]] = counter;
                }

                counter++;
            }

            writer.WriteLine("inline uint64_t indirectTokenMapper(BUToken t) {");
            writer.WriteLine("switch(t){");
            writer.WriteLine("default:");
            writer.WriteLine("case _BUT_EOF: return " + indirectTokenMap[0] + ";");
            foreach (RuleToken t in env.Tokens)
            {
                writer.WriteLine("case " + tokenMapper.StringMap[t] + ": return " + indirectTokenMap[tokenMapper.NumberMap[t]] + ";");
            }
            writer.WriteLine("}");
            writer.WriteLine("}");
            writer.WriteLine();

            // Actual table
            writer.WriteLine("const static table_entry_t actionTable[]={");
            foreach (int state in actionTable.Rows)
            {
                writer.Write("\t");
                foreach (RuleLookahead s in actionTable.Colums)
                {
                    BU.ActionTable.Entry e = actionTable.Get(state, s);
                    if (e == null)
                    {
                        writer.Write("0, ");
                    }
                    else
                    {
                        ulong actionType = 0;
                        switch (e.Action)
                        {
                            case BU.ActionTable.Action.Shift:
                                actionType = 3;
                                break;
                            case BU.ActionTable.Action.Reduce:
                                actionType = 2;
                                break;
                            case BU.ActionTable.Action.Accept:
                                actionType = 1;
                                break;
                        }

                        ulong entry = ((0x3 & actionType) << 62);
                        if (e.Action != BU.ActionTable.Action.Accept)
                            entry |= (0x3FFFFFFFFFFFFFFF & (ulong)e.StateID);

                        writer.Write("0x" + entry.ToString("X") + ", ");
                    }
                }
                writer.WriteLine();
            }
            writer.WriteLine("};");
            writer.WriteLine();
        }

        private void SetupGotoTables(TextWriter writer, Parser.IBUParser context, Environment env, GroupMapper groupMapper)
        {
            // TODO: Make a compressed version!
            // TODO: Make lr(k) version, not only k=1
            BU.GotoTable gotoTable = context.GotoTable;
            int k = 1; //context.K

            writer.WriteLine("/* Goto Table */");
            writer.WriteLine("constexpr int GroupCount = " + groupMapper.NumberMap.Count + ";");

            // Indirect Column Accessor
            int indirectGroupArraySize = (int)System.Math.Pow(groupMapper.NumberMap.Count, k);
            long[] indirectGroupMap = new long[indirectGroupArraySize];
            int counter = 0;
            foreach (RuleGroup grp in env.Groups)
            {
                indirectGroupMap[groupMapper.NumberMap[grp]] = counter;
                counter++;
            }

            writer.WriteLine("const static uint64_t indirectGroupMap[" + indirectGroupArraySize + "]={");
            writer.WriteLine(string.Join(",", indirectGroupMap));
            writer.WriteLine("};");

            // Actual table
            writer.WriteLine("const static state_t gotoTable[]={");
            foreach (int state in context.ActionTable.Rows)
            {
                writer.Write("\t");
                foreach (RuleGroup s in env.Groups)
                {
                    BU.GotoTable.Entry e = gotoTable.Get(state, s);
                    if (e == null)
                    {
                        writer.Write("0, ");
                    }
                    else
                    {
                        ulong entry = (ulong)e.StateID;
                        writer.Write("0x" + entry.ToString("X") + ", ");
                    }
                }
                writer.WriteLine();
            }
            writer.WriteLine("};");
            writer.WriteLine();
        }

        private void SetupGlueTables(TextWriter writer, IBUParser context, PML.Environment env, GroupMapper group_mapper)
        {
            writer.WriteLine("/* Glue Tables */");

            string[] grpsList = new string[env.Rules.Count];
            foreach (Rule rule in env.Rules)
            {
                grpsList[rule.ID] = group_mapper.StringMap[rule.Group];
            }

            int[] betaList = new int[env.Rules.Count];
            foreach (Rule rule in env.Rules)
            {
                betaList[rule.ID] = rule.Tokens.Count;
            }

            // Actual tables
            writer.WriteLine("const static BURuleGroup rule2GroupTable[]={");
            writer.WriteLine(string.Join(",", grpsList));
            writer.WriteLine("};");

            writer.WriteLine("const static uint64_t ruleBetaTable[]={");
            writer.WriteLine(string.Join(",", betaList));
            writer.WriteLine("};");
            writer.WriteLine();
        }

        private void SetupEmbeddedCodeFunctionsHead(TextWriter writer, IBUParser context, PML.Environment env)
        {
            writer.WriteLine("/* Embedded Code Function Declarations */");
            foreach (Rule rule in env.Rules)
            {
                if (string.IsNullOrEmpty(rule.Code))
                    continue;

                writer.WriteLine("\t" + PrintEmbeddedCodeFunctionHead(rule, "") + ";");
            }
            writer.WriteLine();
        }

        private void SetupEmbeddedCodeFunctions(TextWriter writer, IBUParser context, PML.Environment env)
        {
            writer.WriteLine("/* Embedded Code Function Definitions */");
            foreach (Rule rule in env.Rules)
            {
                if (string.IsNullOrEmpty(rule.Code))
                    continue;

                writer.WriteLine(PrintEmbeddedCodeFunctionHead(rule, "Parser") + "{");
                writer.WriteLine(PrintEmbeddedCode(rule));
                writer.WriteLine("}");
                writer.WriteLine();
            }
            writer.WriteLine();
        }
        private string PrintEmbeddedCodeFunctionName(Rule rule)
        {
            return "fn_" + rule.Group.Name + "_" + rule.ID;
        }

        private string PrintEmbeddedCodeFunctionHead(Rule rule, string baseClass)
        {
            if (string.IsNullOrEmpty(rule.Code))
                return "";

            string ret = rule.Group.ReturnType;
            if (string.IsNullOrEmpty(ret))
                ret = "void";
            else
                ret += "*";

            string function_name = PrintEmbeddedCodeFunctionName(rule);
            if (!string.IsNullOrEmpty(baseClass))
                function_name = baseClass + "::" + function_name;

            List<string> paramList = new List<string>();
            int counter = 0;
            foreach (RuleToken t in rule.Tokens)
            {
                if (!string.IsNullOrEmpty(t.ReturnType))
                {
                    string paramName;
                    if (!string.IsNullOrEmpty(t.CodeIdentifier))
                    {
                        paramName = "p_" + t.CodeIdentifier;
                    }
                    else
                    {
                        paramName = "p_" + counter;
                    }

                    paramList.Add(t.ReturnType + "* " + paramName);
                }

                counter++;
            }

            return "inline " + ret + " " + function_name + "(" + string.Join(", ", paramList.ToArray()) + ")";
        }

        private string PrintEmbeddedCode(Rule rule)
        {
            if (string.IsNullOrEmpty(rule.Code))
                return "";

            string output = "";
            string ret = rule.Group.ReturnType;
            if (!string.IsNullOrEmpty(ret))
                output += ret + "* __ret = nullptr;\n";

            output += _EmbeddedGenerator.Generate(rule);

            if (!string.IsNullOrEmpty(ret))
                output += "\nreturn __ret;";

            return output;
        }

        private void SetupParseLoop(TextWriter writer, IBUParser context, PML.Environment env)
        {
            writer.WriteLine("/* Parse Function */");
            writer.WriteLine("ParseResult Parser::parse() {");
            writer.WriteLine("\tvoid* lexerParam = nullptr;");
            if (_Settings.EmbedCustomCode)
                writer.WriteLine("\tstd::stack<void*> paramStack;");
            writer.WriteLine("\tstd::stack<uint64_t> stack;");
            writer.WriteLine("\tstack.push(_BUT_EOF);");
            writer.WriteLine("\tstack.push(" + context.StartState.ID + ");");
            writer.WriteLine("\tBUToken currentToken = nextToken(&lexerParam);");
            if (_Settings.EmbedCustomCode)
                writer.WriteLine("\tif(lexerParam) paramStack.push(lexerParam);");
            writer.WriteLine("\twhile(true){");
            writer.WriteLine("\t\tstate_t state = stack.top();");
            writer.WriteLine("\t\ttable_entry_t entry = actionTable[state*TokenCount + indirectTokenMapper(currentToken)];");
            writer.WriteLine("\t\tActionType action = extractAction(entry);");
            writer.WriteLine("\t\tuint64_t suffix = extractState(entry);");
            writer.WriteLine("\t\tswitch(action){");
            writer.WriteLine("\t\tcase AT_INVALID: return ParseResult{false, nullptr};");
            writer.WriteLine("\t\tcase AT_ACCEPT:");
            if (_Settings.EmbedCustomCode && !string.IsNullOrEmpty(env.Start.ReturnType))
                writer.WriteLine("\t\t\treturn ParseResult{true, paramStack.top()};");
            else
                writer.WriteLine("\t\t\treturn ParseResult{true, nullptr};");
            writer.WriteLine("\t\tcase AT_REDUCE: {");
            if (_Settings.EmbedTraceCode)
                writer.WriteLine("\t\t\tstd::cout << \"R\" << suffix << std::endl;");
            if (_Settings.EmbedCustomCode)
                SetupReduceCodeSection(writer, context, env);
            writer.WriteLine("\t\t\tfor(uint64_t i = 0; i < 2*ruleBetaTable[suffix]; ++i) stack.pop();");
            writer.WriteLine("\t\t\tstate = stack.top();");
            writer.WriteLine("\t\t\tuint64_t grp = rule2GroupTable[suffix];");
            writer.WriteLine("\t\t\tstack.push(grp);");
            writer.WriteLine("\t\t\tstack.push(gotoTable[state*GroupCount + indirectGroupMap[grp]]);");

            writer.WriteLine("\t\t\t} break;");
            writer.WriteLine("\t\tcase AT_SHIFT:");
            if (_Settings.EmbedTraceCode)
                writer.WriteLine("\t\t\tstd::cout << \"S\" << suffix << \" -> \" << currentToken << std::endl;");
            writer.WriteLine("\t\t\tstack.push(currentToken);");
            writer.WriteLine("\t\t\tstack.push(suffix);");
            writer.WriteLine("\t\t\tcurrentToken = nextToken(&lexerParam);");
            if (_Settings.EmbedCustomCode)
                writer.WriteLine("\t\t\tif(lexerParam) paramStack.push(lexerParam);");
            writer.WriteLine("\t\t\tbreak;");
            writer.WriteLine("\t\t}");
            writer.WriteLine("\t}");
            if (_Settings.EmbedCustomCode)
                writer.WriteLine("\treturn ParseResult{true, paramStack.top()};");
            else
                writer.WriteLine("\treturn ParseResult{true, nullptr};");
            writer.WriteLine("};");
            writer.WriteLine();
        }

        private void SetupReduceCodeSection(TextWriter writer, IBUParser context, PML.Environment env)
        {
            writer.WriteLine("\t\t\tswitch(suffix){");
            writer.WriteLine("\t\t\t\tdefault:break;");
            foreach (Rule rule in env.Rules)
            {
                // Make further test to see if rule is really needed to be defined
                if (string.IsNullOrEmpty(rule.Code))
                {
                    bool required = false;
                    foreach (RuleToken t in rule.Tokens)
                    {
                        if (!string.IsNullOrEmpty(t.ReturnType))
                        {
                            required = true;
                            break;
                        }
                    }

                    if (!required)
                        continue;
                }

                // Write case
                writer.WriteLine("\t\t\t\tcase " + rule.ID + ": {");
                List<string> paramList = new List<string>();
                for (int it = rule.Tokens.Count - 1; it >= 0; --it)
                {
                    if (!string.IsNullOrEmpty(rule.Tokens[it].ReturnType))
                    {
                        paramList.Add("p_" + it);
                        string retType = rule.Tokens[it].ReturnType + "*";
                        writer.WriteLine("\t\t\t\t\t" + retType + " p_" + it + " = (" + retType + ")paramStack.top(); ");
                        writer.WriteLine("\t\t\t\t\tparamStack.pop();");
                    }
                }

                paramList.Reverse();

                string ret;
                if (string.IsNullOrEmpty(rule.Group.ReturnType))
                    ret = "";
                else
                    ret = rule.Group.ReturnType + "* __ret = ";

                if (string.IsNullOrEmpty(rule.Code) && !string.IsNullOrEmpty(rule.Group.ReturnType))
                    writer.WriteLine("\t\t\t\t\t" + ret + "nullptr;");
                else
                    writer.WriteLine("\t\t\t\t\t" + ret + PrintEmbeddedCodeFunctionName(rule) + "(" + string.Join(", ", paramList) + ");");

                if (!string.IsNullOrEmpty(rule.Group.ReturnType))
                    writer.WriteLine("\t\t\t\t\tparamStack.push(__ret);");
                writer.WriteLine("\t\t\t\t} break;");
            }
            writer.WriteLine("\t\t\t}");
        }
    }

}
