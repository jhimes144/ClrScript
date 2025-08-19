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
    class VarStmt : Stmt
    {
        public Token Name { get; }

        public Expr Initializer { get; }

        public VarStmt(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitVarStmt(this);
        }
    }
}
