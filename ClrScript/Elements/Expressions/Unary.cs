using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class Unary : Expr
    {
        public Token Op { get; }

        public Expr Right { get; }

        public override Token StartLocation => Op;

        public Unary(Token op, Expr right)
        {
            Op = op;
            Right = right;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitUnary(this);
        }
    }
}
