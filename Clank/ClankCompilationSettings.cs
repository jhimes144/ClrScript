using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank
{
    public class ClankCompilationSettings
    {
        public bool AllowEternal { get; set; }

        public bool AllowRootLevelReturn { get; set; }

        /// <summary>
        /// Optional callback for the print statement. If not specified. Console.WriteLine is used.
        /// </summary>
        public Action<string> PrintCallBack { get; set; } = Console.WriteLine;
    }
}
