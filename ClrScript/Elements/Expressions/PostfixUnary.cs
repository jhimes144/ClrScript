using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class PostfixUnary : Expr
    {
        public Expr Left { get; }
        public Token Op { get; }

        public override Token StartLocation => Left.StartLocation;

        public PostfixUnary(Expr left, Token op)
        {
            Left = left;
            Op = op;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitPostfixUnary(this);
        }
    }
}
