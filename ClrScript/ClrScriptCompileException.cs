using ClrScript.Elements;
using ClrScript.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptCompileException : Exception
    {
        public int? Col { get; }

        public int? Line { get; }

        internal ClrScriptCompileException(string message, InputReader reader) : base(message)
        {
            Col = reader.Column;
            Line = reader.Line;
        }

        internal ClrScriptCompileException(string message, Token token) : base(message)
        {
            Col = token.Column;
            Line = token.Line;
        }

        internal ClrScriptCompileException(string message, Element element) : base(message)
        {
            Col = element.StartLocation.Column;
            Line = element.StartLocation.Line;
        }

        internal ClrScriptCompileException(string message) : base(message) { }
    }
}
