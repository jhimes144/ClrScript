using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class ObjectLiteral : Expr
    {
        public IReadOnlyList<(Token Key, Expr Value)> Properties { get; }

        public Token OpenBrace { get; }

        public override Token StartLocation => OpenBrace;

        public ObjectLiteral(IReadOnlyList<(Token Key, Expr Value)> properties, Token openBrace)
        {
            Properties = properties;
            OpenBrace = openBrace;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitObjectLiteral(this);
        }
    }
}
