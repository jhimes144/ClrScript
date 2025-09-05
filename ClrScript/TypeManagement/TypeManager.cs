using ClrScript.Interop;
using ClrScript.Lexer.TokenReaders;
using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClrScript.TypeManagement
{
    public class TypeManager
    {
        Dictionary<Type, TypeInfo> _typeInfoByType 
            = new Dictionary<Type, TypeInfo>();

        /// <summary>
        /// Insures a given type is valid to be used in ClrScript
        /// </summary>
        /// <param name="type"></param>
        public void ValidateType(Type type)
        {
            if (type == typeof(string))
            {
                return;
            }

            if (type == typeof(double))
            {
                return;
            }

            if (type == typeof(bool))
            {
                return;
            }

            if (InteropHelpers.GetIsSupportedNumericInteropType(type))
            {
                return;
            }

            if (_typeInfoByType.ContainsKey(type))
            {
                return;
            }

            if (!type.IsPublic)
            {
                throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Type must be public and not nested in another class/interface.");
            }

            if (type.IsGenericType)
            {
                throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Generics are not supported.");
            }

            if (type.IsPointer)
            {
                throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Pointers are not supported.");
            }

            if (type.IsInterface)
            {
                throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Interfaces are not supported.");
            }

            var typeInfo = new TypeInfo(this, type);

            var newDict = _typeInfoByType.ToDictionary(p => p.Key, p => p.Value);
            newDict.Add(type, typeInfo);
            _typeInfoByType = newDict;
        }

        /// <summary>
        /// Gets type info for a given type. Returns null if type is object or dynamicnull.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public TypeInfo GetTypeInfo(Type type)
        {
            if (type == typeof(object) || type == typeof(DynamicNull))
            {
                return null;
            }

            if (_typeInfoByType.TryGetValue(type, out var info))
            {
                return info;
            }

            ValidateType(type);
            return _typeInfoByType.GetValueOrDefault(type);
        }
    }
}
