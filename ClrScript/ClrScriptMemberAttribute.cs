using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
    public class ClrScriptMemberAttribute : Attribute
    {
        public bool ConvertToCamelCase { get; set; }

        public string NameOverride { get; set; }

        public string GetMemberName(string actualMemberName)
        {
            if (!string.IsNullOrWhiteSpace(NameOverride))
            {
                actualMemberName = NameOverride;
            }

            if (ConvertToCamelCase)
            {
                actualMemberName = Util.ConvertStrToCamel(actualMemberName);
            }

            return actualMemberName;
        }
    }
}
