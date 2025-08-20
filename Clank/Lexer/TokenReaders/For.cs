using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class For : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.ConsumeKeyword("for");
        }

        public TokenType GetTokenType() => TokenType.For;
    }
}
