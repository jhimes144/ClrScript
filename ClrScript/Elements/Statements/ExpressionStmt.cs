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
    class ExpressionStmt : Stmt
    {
        public Expr Expression { get; }

        public override Token StartLocation => Expression.StartLocation;

        public ExpressionStmt(Expr expr)
        {
            Expression = expr;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitExprStmt(this);
        }
    }
}
