using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    class PropertyAccess : Expr
    {
        public Token Name { get; }

        public Expr Expr { get; }

        public override Token StartLocation => Expr.StartLocation;

        public PropertyAccess(Token name, Expr expr)
        {
            Name = name;
            Expr = expr;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitPropertyAccess(this);
        }
    }
}
