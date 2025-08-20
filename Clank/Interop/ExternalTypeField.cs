using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Interop
{
    class ExternalTypeField
    {
        public string NameOverride { get; set; }

        public FieldInfo Field { get; set; }
    }
}
