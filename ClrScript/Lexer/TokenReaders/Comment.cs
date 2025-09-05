using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Lexer.TokenReaders
{
    class Comment : ITokenReader
    {
        public bool GetIsMatch(TokenReader reader)
        {
            if (!reader.ConsumeMatch("//"))
            {
                return false;
            }

            while (true)
            {
                var c = reader.Consume();

                if (c == '\n' || c.IsNull())
                {
                    break;
                }
            }

            return true;
        }

        public TokenType GetTokenType() => TokenType.Comment;
    }
}