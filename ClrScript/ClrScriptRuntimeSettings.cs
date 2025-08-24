using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptRuntimeSettings
    {
        /// <summary>
        /// Default is no limit. Indicates the maximum amount of time that can pass
        /// before a script is aborted. Aborted scripts throw the <see cref="ClrScriptAbortedException"/> exception.
        /// <para>
        /// <b>Please note the following:</b>
        /// </para>
        /// <para>
        /// - EmitDurationConstraint must be set to true on <see cref="ClrScriptCompilationSettings"/> when used to
        /// compile the script that runs with these settings. See documentation for trade-offs.
        /// </para>
        /// <para>
        /// - ClrScript cannot abort while the script is in the middle of executing app code.
        /// </para>
        /// <para>
        /// - The runtime tries its best to abort as soon as the duration is up, but will likely not be exact to the duration every time.
        /// </para>
        /// </summary>
        public TimeSpan? MaxRunDuration { get; set; }
    }
}
