using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptAbortedException : Exception
    {
        internal ClrScriptAbortedException(string message) : base(message)
        {
        }
    }
}
