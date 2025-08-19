using Clank.Elements.Expressions;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class WhileStmt : Stmt
    {
        public Expr Condition { get; }

        public Stmt Body { get; }

        public WhileStmt(Expr condition, Stmt body)
        {
            Condition = condition;
            Body = body;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitWhileStmt(this);
        }
    }
}
