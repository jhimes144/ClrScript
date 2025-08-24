using ClrScript.Elements;
using ClrScript.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptCompileError
    {
        public bool IsWarning { get; }

        public string Message { get; }

        public int? Col { get; }

        public int? Line { get; }

        internal ClrScriptCompileError(string message, InputReader reader, bool isWarning = false)
        {
            Col = reader.Column;
            Line = reader.Line;
            Message = message;
            IsWarning = isWarning;
        }

        internal ClrScriptCompileError(string message, Token token, bool isWarning = false)
        {
            Col = token.Column;
            Line = token.Line;
            Message = message;
            IsWarning = isWarning;
        }

        internal ClrScriptCompileError(string message, Element element, bool isWarning = false)
        {
            Col = element.StartLocation.Column;
            Line = element.StartLocation.Line;
            Message = message;
            IsWarning = isWarning;
        }

        public ClrScriptCompileError(string message, int col, int line, bool isWarning = false)
        {
            Col = col;
            Line = line;
            Message = message;
            IsWarning = isWarning;
        }

        public ClrScriptCompileError(string message, bool isWarning = false)
        {
            Message = message;
            IsWarning = isWarning;
        }
    }
}
