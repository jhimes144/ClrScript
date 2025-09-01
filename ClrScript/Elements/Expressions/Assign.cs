using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class Assign : Expr
    {
        public Expr AssignTo { get; }

        public Expr ExprAssignValue { get; }

        public override Token StartLocation => AssignTo.StartLocation;

        public Assign(Expr assignTo, Expr expression)
        {
            AssignTo = assignTo;
            ExprAssignValue = expression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitAssign(this);
        }
    }
}
