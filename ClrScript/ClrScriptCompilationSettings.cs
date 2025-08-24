using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptCompilationSettings
    {
        /// <summary>
        /// Default is true. Indicates whether the forever modifier can be used on variables.
        /// Forever variables persist their values between script invocations for a given context. This is the only
        /// feature built in to ClrScript that persists user generated data between script invocations.
        /// </summary>
        public bool AllowForever { get; set; } = true;

        /// <summary>
        /// Default is true. Indicates whether the return keyword is allowed at the root level of a script.
        /// Scripts that do not include a root level return can still return implicitly. (Its still up to you
        /// whether its handled by app code)
        /// </summary>
        public bool AllowRootLevelReturn { get; set; } = true;

        /// <summary>
        /// Default value is true. Indicates whether object literals are allowed in script code.
        /// </summary>
        public bool AllowUserObjectConstruction { get; set; } = true;

        /// <summary>
        /// Default value is true. Indicates whether array literals are allowed in script code.
        /// </summary>
        public bool AllowUserArrayConstruction { get; set; } = true;

        /// <summary>
        /// Default value is true. Indicates whether the print statement is allowed.
        /// </summary>
        public bool AllowPrintStatement { get; set; } = true;

        /// <summary>
        /// Default value is false. This property must be true to allow the MaxRunDuration
        /// setting in <see cref="ClrScriptRuntimeSettings"/>. Emits the required byte code to enforce the MaxRunDuration constraint.
        /// Setting this to true imposes a runtime performance hit. Its recommended that you benchmark with the setting on and off
        /// to assess.
        /// </summary>
        public bool EmitDurationConstraint { get; set; }
    }
}
