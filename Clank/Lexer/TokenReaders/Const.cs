using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class Const : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.ConsumeKeyword("const");
        }

        public TokenType GetTokenType() => TokenType.Const;
    }
}
