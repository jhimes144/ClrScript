using Clank.Elements.Expressions;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class IfStmt : Stmt
    {
        public Expr Condition { get; }

        public Stmt ThenBranch { get; }

        public Stmt ElseBranch { get; }

        public IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitIfStmt(this);
        }
    }
}
