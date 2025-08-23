using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ClrScriptPropertyAttribute : Attribute
    {
        public bool ConvertToCamelCase { get; set; }

        public string NameOverride { get; set; }
    }
}
