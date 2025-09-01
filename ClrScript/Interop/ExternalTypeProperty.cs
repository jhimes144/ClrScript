using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Interop
{
    class ExternalTypeProperty : ExternalTypeMember
    {
        public PropertyInfo Property { get; set; }

        public override MemberInfo MemberInfo => Property;
    }
}
