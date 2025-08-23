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
    class IfStmt : Stmt
    {
        public Expr Condition { get; }

        public Stmt ThenBranch { get; }

        public Stmt ElseBranch { get; }

        public override Token StartLocation { get; }

        public IfStmt(Token startLoc, Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            StartLocation = startLoc;
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
