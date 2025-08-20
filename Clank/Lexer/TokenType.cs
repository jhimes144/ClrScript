using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer
{
    enum TokenType
    {
    // Keywords
    Const,
    Var,
    If,
    For,
    Else,
    While,
    Return,

    // Literals
    Number,
    String,
    Identifier,
    True,
    False,
    Null,

    // Operators
    Plus,
    Minus,
    Multiply,
    Divide,
    Assign,
    Equal,
    EqualEqual,
    Dot,
    Bang,
    BangEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Arrow,

    // Punctuation
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    SemiColon,
    Colon,
    Ampersand,
    Pipe,
    DoublePipe,
    DoubleAmpersand,
    Comma,


    // Special
    EOF,
}
}
