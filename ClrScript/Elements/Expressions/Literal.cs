using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    class Literal : Expr
    {
        public object Value { get; }

        public Token Token { get; }

        public override Token StartLocation => Token;

        public Literal(object value, Token token = null)
        {
            Value = value;
            Token = token;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitLiteral(this);
        }
    }
}
