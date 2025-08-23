using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    /// <summary>
    /// Allows custom implementation of the print statement in ClrScript.
    /// </summary>
    public interface IImplementsPrintStmt
    {
        void Print(object obj);
    }
}
