using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    class Logical : Expr
    {
        public Expr Left { get; }
        public Token Op { get; }
        public Expr Right { get; }

        public Logical(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitLogical(this);
        }
    }
}
