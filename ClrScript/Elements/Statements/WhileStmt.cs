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
    class WhileStmt : Stmt
    {
        public Expr Condition { get; }

        public Stmt Body { get; }

        public override Token StartLocation { get; }

        public WhileStmt(Token startLoc, Expr condition, Stmt body)
        {
            StartLocation = startLoc;
            Condition = condition;
            Body = body;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitWhileStmt(this);
        }
    }
}
