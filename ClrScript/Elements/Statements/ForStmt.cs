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
    class ForStmt : Stmt
    {
        public Stmt Initializer { get; }
        public Expr Condition { get; }
        public Expr Increment { get; }
        public Stmt Body { get; }

        public override Token StartLocation { get; }

        // TODO: Continue and break statements

        public ForStmt(Token startLoc, Stmt initializer, Expr condition, Expr increment, Stmt body)
        {
            StartLocation = startLoc;
            Initializer = initializer;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitForStmt(this);
        }
    }
}
