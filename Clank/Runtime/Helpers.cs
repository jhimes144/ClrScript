using Clank.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Runtime
{
    public static class Helpers
    {
        /// <summary>
        /// Returns the System.Type for any object, including null.
        /// </summary>
        public static Type GetTypeIncludeNull(this object obj)
        {
            if (obj == null)
            {
                return typeof(DynamicNull);
            }

            return obj.GetType();
        }
    }
}
