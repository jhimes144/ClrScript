using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class BlueprintStmt : Stmt
    {
        public override Token StartLocation => Name;

        public Token Name { get; }

        public IReadOnlyList<Stmt> Statements { get; }

        public BlueprintStmt(Token name, IReadOnlyList<Stmt> statements)
        {
            Name = name;
            Statements = statements;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlueprintStmt(this);
        }
    }
}
