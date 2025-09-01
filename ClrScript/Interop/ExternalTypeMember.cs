using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Interop
{
    abstract class ExternalTypeMember
    {
        public string NameOverride { get; set; }

        public abstract MemberInfo MemberInfo { get; }
    }
}
