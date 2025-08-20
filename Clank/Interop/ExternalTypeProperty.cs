using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Interop
{
    class ExternalTypeProperty
    {
        public string NameOverride { get; set; }

        public PropertyInfo Property { get; set; }
    }
}
