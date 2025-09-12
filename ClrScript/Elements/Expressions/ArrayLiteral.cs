using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class ArrayLiteral : Expr
    {
        public override Token StartLocation { get; }

        public IReadOnlyList<Expr> Contents { get; }

        public ArrayLiteral(Token startLocation, IReadOnlyList<Expr> contents)
        {
            StartLocation = startLocation;
            Contents = contents;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitArrayLiteral(this);
        }
    }
}
