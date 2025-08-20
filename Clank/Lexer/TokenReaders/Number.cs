﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    class Number : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            var foundDigit = false;
            var foundDot = false;

            while (true)
            {
                var c = reader.Peek();

                if (char.IsDigit(c))
                {
                    reader.Consume();
                    foundDigit = true;
                    continue;
                }

                if (c == '_' && foundDigit)
                {
                    reader.Skip();
                    continue;
                }

                if (c == '.')
                {
                    if (!foundDot)
                    {
                        reader.Consume();
                        foundDot = true;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (foundDigit)
                {
                    return true;
                }

                return false;
            }
        }

        public TokenType GetTokenType() => TokenType.Number;
    }
}
