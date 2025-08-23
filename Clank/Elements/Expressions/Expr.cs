using Clank.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements.Expressions
{
    abstract class Expr : Element
    {
        public Type InferredType { get; set; }

        public abstract void Accept(IExpressionVisitor visitor);
    }
}
