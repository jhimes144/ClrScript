using Clank.Elements.Expressions;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class ReturnStmt : Stmt
    {
        public Expr Expression { get; }

        public Type Type { get; set; }

        public ReturnStmt(Expr expression)
        {
            Expression = expression;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitReturnStmt(this);
        }
    }
}
