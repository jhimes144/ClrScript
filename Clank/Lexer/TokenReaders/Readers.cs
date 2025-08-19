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
                new GreaterThan(),
                new GreaterThanOrEqual(),
                new Number(),
                new Identifier(),
                new LeftBrace(),
                new LeftParen(),
                new LessThan(),
                new LessThanOrEqual(),
                new Minus(),
                new Multiply(),
                new Plus(),
                new RightBrace(),
                new RightParen(),
                new SemiColon(),
                new Colon(),
                new String(),
            };
        }
    }
}
