using ClrScript.Interop;
using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Runtime
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

        public static string GetClrScriptTypeDisplay(this object obj)
        {
            var type = GetTypeIncludeNull(obj);
            return GetClrScriptTypeDisplay(type);
        }

        public static bool GetIsAssignableTo(Type fromType, Type toType)
        { 
            return toType.IsAssignableFrom(fromType);
        }

        public static string GetClrScriptTypeDisplay(this Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(bool))
            {
                return "bool";
            }
            else if (InteropHelpers.GetIsSupportedNumericInteropType(type))
            {
                return "number";
            }
            else if (type == typeof(DynamicNull))
            {
                return "null";
            }
            else if (typeof(Delegate).IsAssignableFrom(type) || type == typeof(MethodInfo))
            {
                return "function";
            }

            if (typeof(IEnumerable<>).IsAssignableFrom(type))
            {
                var arrayType = type.GenericTypeArguments[0];
                return GetClrScriptTypeDisplay(arrayType) + "[]";
            }

            return "object";
        }
    }
}
