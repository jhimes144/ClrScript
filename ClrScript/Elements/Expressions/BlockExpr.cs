using ClrScript.Elements.Statements;
using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class BlockExpr : Expr
    {
        public Block Block { get; }

        public override Token StartLocation => Block.StartLocation;

        public BlockExpr(Block block)
        {
            Block = block;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitBlockExpr(this);
        }
    }
}
