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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PML.Grammar
{
    public enum ErrorType
    {
        Lexer_StringNotClosed,
        Lexer_InvalidOperator,
        Lexer_UnknownIdentifier,
        Lexer_UnknownCharacter,
        Lexer_InvalidComment,
        Parser_WrongToken,
        Internal_LookParamLessOne,
    }

    [Serializable()]
    public class ParserError : Exception, ISerializable
    {
        private ErrorType _Type;
        private Object[] _Values;

        public ErrorType Type
        {
            get
            {
                return _Type;
            }
        }

        public ParserError(ErrorType type)
        {
            _Type = type;
        }

        public ParserError(ErrorType type, Object val)
        {
            _Type = type;
            _Values = new Object[] { val };
        }

        public ParserError(ErrorType type, Object val1, Object val2)
        {
            _Type = type;
            _Values = new Object[] { val1, val2 };
        }

        public ParserError(ErrorType type, Object val1, Object val2, Object val3)
        {
            _Type = type;
            _Values = new Object[] { val1, val2, val3 };
        }

        public ParserError(ErrorType type, Object val1, Object val2, Object val3, Object val4)
        {
            _Type = type;
            _Values = new Object[] { val1, val2, val3, val4 };
        }

        public Object Value(int i)
        {
            return _Values[i];
        }

        protected ParserError(SerializationInfo info, StreamingContext context) :
             base( info, context )
        {
            _Type = (ErrorType)info.GetInt32("ERR_Type");
            _Values = info.GetValue("ERR_Values", _Values.GetType()) as object[];
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("ERR_Type", (int)_Type);
            info.AddValue("ERR_Values", _Values);

            base.GetObjectData(info, context);
        }
    }
}
