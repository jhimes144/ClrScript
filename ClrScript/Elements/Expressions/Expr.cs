using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Expressions
{
    abstract class Expr : Element
    {
        public abstract void Accept(IExpressionVisitor visitor);
    }
}
