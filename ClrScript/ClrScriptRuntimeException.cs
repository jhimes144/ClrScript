using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    class ClrScriptRuntimeException : Exception
    {
        internal ClrScriptRuntimeException(string message) : base(message) { }
    }
}
