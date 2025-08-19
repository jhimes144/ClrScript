using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class EqualEqual : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            return reader.Consume() == '=' && reader.Consume() == '=';
        }

        public TokenType GetTokenType() => TokenType.EqualEqual;
    }
}
