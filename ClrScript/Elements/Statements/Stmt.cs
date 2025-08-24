using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Statements
{
    abstract class Stmt : Element
    {
        public abstract void Accept(IStatementVisitor visitor);
    }
}
