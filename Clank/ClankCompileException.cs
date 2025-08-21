using Clank.Elements;
using Clank.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank
{
    public class ClankCompileException : Exception
    {
        public int? Col { get; }

        public int? Line { get; }

        internal ClankCompileException(string message, InputReader reader) : base(message)
        {
            Col = reader.Column;
            Line = reader.Line;
        }

        internal ClankCompileException(string message, Token token) : base(message)
        {
            Col = token.Column;
            Line = token.Line;
        }

        internal ClankCompileException(string message, Element element) : base(message)
        {
            Col = element.StartLocation.Column;
            Line = element.StartLocation.Line;
        }

        internal ClankCompileException(string message) : base(message) { }
    }
}
