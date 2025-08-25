using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    enum RootMemberAccessType
    {
        Variable,
        External
    }

    class MemberRootAccess : Expr
    {
        public Token Name { get; }

        public override Token StartLocation => Name;

        public RootMemberAccessType AccessType { get; set; }

        public PropertyInfo ExternalProperty { get; set; }

        public MemberRootAccess(Token name)
        {
            Name = name;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.VisitMemberRootAccess(this);
        }
    }
}
