using Clank.Elements.Expressions;
using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class PrintStmt : Stmt
    {
        public override Token StartLocation => Expression.StartLocation;

        public Expr Expression { get; }

        public PrintStmt(Expr expression)
        {
            Expression = expression;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitPrintStmt(this);
        }
    }
}
