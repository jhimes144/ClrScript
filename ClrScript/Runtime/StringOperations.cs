using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Runtime
{
    public static class StringOperations
    {
        [ClrScriptMember(ConvertToCamelCase = true)]
        public static double Length(string str)
        {
            return str.Length;
        }


    }
}
