using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Lexer.TokenReaders
{
    class LeftBracket : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.Consume() == '[';
        }

        public TokenType GetTokenType() => TokenType.LeftBracket;
    }
}
