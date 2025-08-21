using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class BlueprintPropertyStmt : Stmt
    {
        public override Token StartLocation => Type;

        public Token Type { get; }

        public Token Name { get; }

        public BlueprintPropertyStmt(Token type, Token name)
        {
            Type = type;
            Name = name;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlueprintPropStmt(this);
        }
    }
}
