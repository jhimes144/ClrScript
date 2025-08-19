using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Runtime
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class ClankNameOverrideAttribute : Attribute
    {
        public string Name { get; set; }

        public ClankNameOverrideAttribute(string name)
        {
            Name = name;
        }
    }
}
