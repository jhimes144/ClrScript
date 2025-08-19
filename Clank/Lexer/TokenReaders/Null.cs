using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class Null : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.ConsumeKeyword("null");
        }

        public TokenType GetTokenType() => TokenType.Null;
    }
}
