using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank
{
    public enum NumberPrecision
    {
        /// <summary>
        /// Clank numbers will be expressed as double-precision 64-bit floating-point numbers, (double) in C#
        /// Any interop will be casted
        /// </summary>
        DoublePrecision,

        /// <summary>
        /// Clank numbers will be expressed as single-precision 32-bit floating-point numbers, (float) in C#
        /// Any interop will be casted
        /// </summary>
        SinglePrecision
    }

    public class ClankCompilationSettings
    {
        public NumberPrecision NumberPrecision { get; set; }

        public bool AllowEternal { get; set; }

        public bool AllowRootLevelReturn { get; set; }
    }
}
