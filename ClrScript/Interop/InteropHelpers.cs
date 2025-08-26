using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Interop
{
    public static class InteropHelpers
    {
        static readonly Type[] _supportedNumericInteropTypes =
        {
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(uint),
            typeof(ulong),
            typeof(ushort),
            typeof(byte),
            typeof(sbyte),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        public static bool GetIsSupportedNumericInteropType(Type type)
        {
            return _supportedNumericInteropTypes.Contains(type);
        }
    }
}
