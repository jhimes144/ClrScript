using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class Return : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.ConsumeKeyword("return");
        }

        public TokenType GetTokenType() => TokenType.Return;
    }
}