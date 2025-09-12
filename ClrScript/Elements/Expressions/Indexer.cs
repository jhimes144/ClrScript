using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class Indexer : Expr
    {
        public override Token StartLocation { get; }

        public Expr Expression { get; }

        public Indexer(Token startLocation, Expr expression)
        {
            StartLocation = startLocation;
            Expression = expression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.visitIndexer(this);
        }
    }
}
