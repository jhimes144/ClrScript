using Clank.Lexer.TokenReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer
{
    class TokenReader
    {
        readonly InputReader _input;
        readonly StringBuilder _tokenBuffer = new StringBuilder();

        public int Index { get; private set; }

        public TokenReader(InputReader reader)
        {
            _input = reader;
        }

        public void Reset()
        {
            Index = 0;
            _tokenBuffer.Clear();
        }

        public bool IsAtEnd()
        {
            return _input.IsAtEnd();
        }

        public char Consume()
        {
            var c = _input.Peek(Index);
            Index++;
            _tokenBuffer.Append(c);
            return c;
        }
        
        public char Peek()
        {
            return _input.Peek(Index);
        }

        public bool PeakIsDelimiter()
        {
            var c = _input.Peek(Index);
            return !char.IsLetterOrDigit(c) && c != '_';
        }

        public bool ConsumeKeyword(string keyword)
        {
            var hasKeyword = ConsumeMatch(keyword);

            if (hasKeyword)
            {
                return PeakIsDelimiter();
            }

            return false;
        }

        public bool ConsumeMatch(string match)
        {
            for (int i = 0; i < match.Length; i++)
            {
                char c = Consume();

                if (c.IsNull() || c != match[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return _tokenBuffer.ToString();
        }
    }
}
