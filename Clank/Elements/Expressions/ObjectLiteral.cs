using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    class ObjectLiteral : Expr
    {
        public IReadOnlyList<(Token Key, Expr Value)> Properties { get; }

        public ObjectLiteral(IReadOnlyList<(Token Key, Expr Value)> properties)
        {
            Properties = properties;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitObjectLiteral(this);
        }
    }
}
