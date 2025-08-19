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

        public Block(IReadOnlyList<Stmt> statements)
        {
            Statements = statements;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlock(this);
        }
    }
}
