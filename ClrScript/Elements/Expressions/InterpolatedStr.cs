using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class InterpolatedStr : Expr
    {
        public IReadOnlyList<Expr> Expressions { get; }

        public override Token StartLocation { get; }

        public InterpolatedStr(IReadOnlyList<Expr> expressions, Token startLocation)
        {
            Expressions = expressions;
            StartLocation = startLocation;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitInterpolatedString(this);
        }
    }
}
