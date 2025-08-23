using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank
{
    class ClankRuntimeException : Exception
    {
        internal ClankRuntimeException(string message) : base(message) { }
    }
}
