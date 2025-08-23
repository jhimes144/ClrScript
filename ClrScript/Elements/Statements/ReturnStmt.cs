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
    class ReturnStmt : Stmt
    {
        public Expr Expression { get; }

        public Type Type { get; set; }

        public override Token StartLocation { get; }

        public ReturnStmt(Token startLoc, Expr expression)
        {
            StartLocation = startLoc;
            Expression = expression;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitReturnStmt(this);
        }
    }
}
