using Clank.Lexer.TokenReaders;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Clank;
using System.Collections.Generic;

namespace Clank.Lexer
{
    class ClankLexer
    {
        readonly InputReader _reader;
        readonly TokenReader _tokenReader;
        readonly ITokenReader[] _tokenReaders;

        public ClankLexer(string input)
        {
            _reader = new InputReader(normalizeLineEndings(input));
            _tokenReader = new TokenReader(_reader);

            _tokenReaders = Readers.Get();
        }

        public IReadOnlyList<Token> Tokenize()
        {
            var tokens = new List<Token>();

            if (!_reader.IsAtEnd())
            {
                _reader.AdvanceRemainingWhiteSpace();
            }

            while (!_reader.IsAtEnd())
            {
                var tokenFound = false;
                var readerStartPos = _reader.Pos;

                foreach (var reader in _tokenReaders)
                {
                    _tokenReader.Reset();
                    var match = reader.GetIsMatch(_tokenReader);

                    if (match)
                    {
                        tokens.Add(new Token(reader.GetTokenType(), 
                            _tokenReader.ToString(), _reader.Line, _reader.Pos));

                        _reader.Advance(_tokenReader.Index);
                        _reader.AdvanceRemainingWhiteSpace();
                        tokenFound = true;
                        break;
                    }
                }

                if (!tokenFound)
                {
                    throw new ClankCompileException("Unexpected character(s)", _reader);
                }
            }

            tokens.Add(new Token(TokenType.EOF, string.Empty, _reader.Line, _reader.Column));
            return tokens;
        }

        static string normalizeLineEndings(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }
    }
}
