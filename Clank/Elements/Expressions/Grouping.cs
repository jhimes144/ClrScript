using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    class Grouping : Expr
    {
        public Expr Expression { get; }

        public override Token StartLocation => Expression.StartLocation;

        public Grouping(Expr expression)
        {
            Expression = expression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitGrouping(this);
        }
    }
}
