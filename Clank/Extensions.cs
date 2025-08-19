using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank
{
    static class Extensions
    {
        public static bool IsNull(this char c)
        {
            return c == '\0';
        }
    }
}
