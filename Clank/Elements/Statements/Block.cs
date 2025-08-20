using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Statements
{
    class Block : Stmt
    {
        public IReadOnlyList<Stmt> Statements { get; }

        public Token OpenBrace { get; }

        public override Token StartLocation => OpenBrace;

        public Block(IReadOnlyList<Stmt> statements, Token openBrace = null)
        {
            Statements = statements;
            OpenBrace = openBrace;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlock(this);
        }
    }
}
