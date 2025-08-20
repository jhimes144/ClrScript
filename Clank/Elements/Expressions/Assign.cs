using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    class Assign : Expr
    {
        public Token Name { get; }

        public Expr Expression { get; }

        public override Token StartLocation => Name;

        public Assign(Token name, Expr expression)
        {
            Name = name;
            Expression = expression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitAssign(this);
        }
    }
}
