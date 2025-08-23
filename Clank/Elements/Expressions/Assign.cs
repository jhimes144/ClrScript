using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    class Assign : Expr
    {
        public Expr AssignTo { get; }

        public Expr Expression { get; }

        public override Token StartLocation => AssignTo.StartLocation;

        public Assign(Expr assignTo, Expr expression)
        {
            AssignTo = assignTo;
            Expression = expression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitAssign(this);
        }
    }
}
