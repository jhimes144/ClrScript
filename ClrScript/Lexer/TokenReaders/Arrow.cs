using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Lexer.TokenReaders
{
    internal class Arrow : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            if (reader.Consume() != '=')
            {
                return false;
            }

            if (reader.Consume() != '>')
            {
                return false;
            }

            return true;
        }

        public TokenType GetTokenType() => TokenType.Arrow;
    }
}
