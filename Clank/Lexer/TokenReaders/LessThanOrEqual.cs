using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class LessThanOrEqual : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            if (reader.Consume() != '<')
            {
                return false;
            }

            if (reader.Consume() != '=')
            {
                return false;
            }

            return true;
        }

        public TokenType GetTokenType() => TokenType.LessThanOrEqual;
    }
}
