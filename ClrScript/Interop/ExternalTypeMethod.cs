using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Interop
{
    class ExternalTypeMethod : ExternalTypeMember
    {
        public MethodInfo Method { get; set; }
    }
}
