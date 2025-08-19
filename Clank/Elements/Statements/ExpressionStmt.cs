using Clank.Elements.Expressions;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class ExpressionStmt : Stmt
    {
        public Expr Expression { get; }

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
