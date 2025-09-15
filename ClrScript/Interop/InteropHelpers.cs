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

        static readonly Type[] _supportedGenerics =
        {
            typeof(IList<>),
            typeof(IReadOnlyList<>),
            typeof(IEnumerable<>),
            typeof(Nullable<>)
        };

        public static bool GetIsSupportedGenericType(Type type)
        {
            foreach (var gType in _supportedGenerics)
            {
                if (gType.MakeGenericType(type.GenericTypeArguments).IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GetIsSupportedValueType(Type type)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GenericTypeArguments[0];
            }

            return type == typeof(bool) || _supportedNumericInteropTypes.Contains(type);
        }

        public static bool GetIsSupportedNumericInteropType(Type type, bool includeNullables = false)
        {
            if (includeNullables && 
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return _supportedNumericInteropTypes.Contains(type.GenericTypeArguments[0]);
            }

            return _supportedNumericInteropTypes.Contains(type);
        }

        public static bool GetIsSupportedNumericInteropTypeNeedingConversion(Type type, bool includeNullables = false)
        {
            if (type == typeof(double))
            {
                return false;
            }

            return GetIsSupportedNumericInteropType(type, includeNullables);
        }

        public static object ConvertDynBoxNumeric(double value, Type toType)
        {
            if (toType == typeof(double))
            {
                return value;
            }

            if (toType == typeof(int))
                return (int)value;
            else if (toType == typeof(long))
                return (long)value;
            else if (toType == typeof(short))
                return (short)value;
            else if (toType == typeof(uint))
                return (uint)value;
            else if (toType == typeof(ulong))
                return (ulong)value;
            else if (toType == typeof(ushort))
                return (ushort)value;
            else if (toType == typeof(byte))
                return (byte)value;
            else if (toType == typeof(sbyte))
                return (sbyte)value;
            else if (toType == typeof(float))
                return (float)value;
            else if (toType == typeof(decimal))
                return (decimal)value;
            else
                throw new InvalidOperationException($"Unexpected numeric type: {toType.Name}");
        }
    }
}
