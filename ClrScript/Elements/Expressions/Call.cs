using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class Call : Expr
    {
        public override Token StartLocation { get; }
        public Expr Callee { get; }
        public IReadOnlyList<Expr> Arguments { get; }

        public Call(Token startLocation, Expr callee, IReadOnlyList<Expr> arguments)
        {
            StartLocation = startLocation;
            Callee = callee;
            Arguments = arguments;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitCall(this);
        }
    }
}
