using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Interop
{
    class ExternalTypeField : ExternalTypeMember
    {
        public FieldInfo Field { get; set; }

        public override MemberInfo MemberInfo => Field;
    }
}
