using ClrScript.Elements.Expressions;
using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Statements
{
    class AssignStmt : Stmt
    {
        public Expr AssignTo { get; }

        public Expr ExprAssignValue { get; }

        public override Token StartLocation => AssignTo.StartLocation;

        public AssignStmt(Expr assignTo, Expr expression)
        {
            AssignTo = assignTo;
            ExprAssignValue = expression;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitAssignStmt(this);
        }
    }
}
