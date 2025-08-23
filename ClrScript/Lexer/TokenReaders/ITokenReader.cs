using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Lexer.TokenReaders
{
    interface ITokenReader
    {
        bool GetIsMatch(TokenReader reader);
        TokenType GetTokenType();
    }
}
