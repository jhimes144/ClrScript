using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Lexer.TokenReaders
{
    static class Readers
    {
        public static ITokenReader[] Get()
        {
            return new ITokenReader[]
            {
                new If(),
                new Else(),
                new While(),
                new Return(),
                new Print(),
                new For(),
                new True(),
                new False(),
                new Null(),
                new Arrow(),
                new BangEqual(),
                new Bang(),
                new Const(),
                new Var(),
                new DoublePipe(),
                new DoubleAmpersand(),
                new Pipe(),
                new Ampersand(),
                new Divide(),
                new Dot(),
                new Comma(),
                new EqualEqual(),
                new Equal(),
                new GreaterThanOrEqual(),
                new GreaterThan(),
                new Number(),
                new Identifier(),
                new LeftBrace(),
                new LeftBracket(),
                new LeftParen(),
                new LessThanOrEqual(),
                new LessThan(),
                new Minus(),
                new Multiply(),
                new Plus(),
                new RightBrace(),
                new RightBracket(),
                new RightParen(),
                new SemiColon(),
                new Colon(),
                new String(),
            };
        }
    }
}
