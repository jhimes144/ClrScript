using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Clank.Runtime.Builtins
{
    public class ClankArray
    {
        readonly List<object> _contents = new List<object>();

#if NET9_0
        readonly Lock _lock = new();
#else
        readonly object _lock = new object();
#endif

    }
}
