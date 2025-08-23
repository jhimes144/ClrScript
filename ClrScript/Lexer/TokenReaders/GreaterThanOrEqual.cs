using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Lexer.TokenReaders
{
    class GreaterThanOrEqual : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.Consume() == '>' && reader.Consume() == '=';
        }

        public TokenType GetTokenType() => TokenType.GreaterThanOrEqual;
    }
}
