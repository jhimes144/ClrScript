using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptInteropException : Exception
    {
        internal ClrScriptInteropException(string message) : base(message) { }
    }
}
