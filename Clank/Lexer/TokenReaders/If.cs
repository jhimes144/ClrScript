using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class If : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.ConsumeKeyword("if");
        }

        public TokenType GetTokenType() => TokenType.If;
    }
}
