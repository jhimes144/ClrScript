using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ClankPropertyAttribute : Attribute
    {
        public bool ConvertToCamelCase { get; set; }

        public string NameOverride { get; set; }
    }
}
