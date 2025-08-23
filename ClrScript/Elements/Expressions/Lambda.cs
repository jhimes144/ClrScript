using ClrScript.Lexer;
using ClrScript.Elements.Statements;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class Lambda : Expr
    {
        public IReadOnlyList<Token> Parameters { get; }

        public Expr Body { get; }

        public override Token StartLocation => Parameters.Count > 0 ? Parameters[0] : Body.StartLocation;

        public Lambda(IReadOnlyList<Token> parameters, Expr body)
        {
            Parameters = parameters;
            Body = body;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitLambda(this);
        }
    }
}
