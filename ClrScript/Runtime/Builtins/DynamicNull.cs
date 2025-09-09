using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Runtime.Builtins
{
    /// <summary>
    /// Represents the type of a null value.
    /// </summary>
    [ClrScriptType]
    public sealed class DynamicNull
    {
        /// <summary>
        /// Private constructor is never called since 'null' is the only valid instance.
        /// </summary>
        private DynamicNull() { }
    }
}
