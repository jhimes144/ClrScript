using Clank.Lexer;
using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    class Variable : Expr
    {
        public Token Name { get; }

        public override Token StartLocation => Name;

        public Variable(Token name)
        {
            Name = name;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitVariable(this);
        }
    }
}
