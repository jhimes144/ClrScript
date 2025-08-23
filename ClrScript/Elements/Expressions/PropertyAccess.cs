using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class MemberAccess : Expr
    {
        public Token Name { get; }

        public Expr Expr { get; }

        public override Token StartLocation => Expr.StartLocation;

        public MemberAccess(Token name, Expr expr)
        {
            Name = name;
            Expr = expr;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitMemberAccess(this);
        }
    }
}
