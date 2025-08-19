using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class Identifier : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            var foundDigitOrLetter = false;

            while (true)
            {
                var c = reader.Peek();

                if (char.IsDigit(c) || char.IsLetter(c))
                {
                    foundDigitOrLetter = true;
                    reader.Consume();
                    continue;
                }

                if (foundDigitOrLetter)
                {
                    return true;
                }

                return false;
            }
        }

        public TokenType GetTokenType() => TokenType.Identifier;
    }
}
