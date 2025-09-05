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
    class PostFixUnaryAssignStmt : Stmt
    {
        public Expr Left { get; }
        public Token Op { get; }

        public override Token StartLocation => Left.StartLocation;

        public PostFixUnaryAssignStmt(Expr left, Token op)
        {
            Left = left;
            Op = op;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitPostFixUnaryAssignStmt(this);
        }
    }
}
