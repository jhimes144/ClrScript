using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptRuntimeException : Exception
    {
        public ClrScriptRuntimeException(string message) : base(message) { }

        public ClrScriptRuntimeException(Exception inner, string message) : base(message, inner) { }
    }
}
