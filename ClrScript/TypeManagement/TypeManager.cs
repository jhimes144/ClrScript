using ClrScript.Interop;
using ClrScript.Lexer.TokenReaders;
using ClrScript.Runtime;
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
        static readonly Type[] _allowedGenerics =
        {
            typeof(IList<>),
            typeof(IReadOnlyList<>),
            typeof(IEnumerable<>),
        };

        Dictionary<Type, TypeInfo> _typeInfoByType 
            = new Dictionary<Type, TypeInfo>();

        List<MethodInfo> _registeredExtensions = new List<MethodInfo>();

        /// <summary>
        /// Insures a given type is valid to be used in ClrScript.
        /// </summary>
        /// <param name="type"></param>
        public void ValidateType(Type type, bool asExtension = false)
        {
            if (type == typeof(object))
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

            // this we may support later
            if (type == typeof(char))
            {
                throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Char type is not supported.");
            }

            if (!type.IsPublic)
            {
                throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Type must be public and not nested in another class/interface.");
            }

            if (type.IsGenericType)
            {
                var foundSupported = false;

                foreach (var gType in _allowedGenerics)
                {
                    if (gType.MakeGenericType(type.GenericTypeArguments).IsAssignableFrom(type))
                    {
                        foundSupported = true;
                        break;
                    }
                }

                if (!foundSupported)
                {
                    throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Type is not included in list of supported generics.");
                }
            }

            if (type.IsPointer)
            {
                throw new ClrScriptInteropException($"'{type}' is an invalid ClrScript type. Pointers are not supported.");
            }

            var membersByName = new Dictionary<string, MemberInfo>();

            if (asExtension)
            {
                populateMembersFrom(type, true, membersByName);
                var newExtensions = membersByName.Values.Cast<MethodInfo>().ToArray();

                foreach (var extensionMethod in newExtensions)
                {
                    var exType = extensionMethod.GetParameters()[0].ParameterType;

                    if (_typeInfoByType.TryGetValue(exType, out var exTypeInfo))
                    {
                        exTypeInfo.OverlayExtensions(newExtensions);
                    }
                }

                _registeredExtensions.AddRange(newExtensions);
            }
            else
            {
                if (!typeof(ClrScriptObject).IsAssignableFrom(type)
                    && !typeof(ClrScriptArray).IsAssignableFrom(type)
                    && type != typeof(string) 
                    && type != typeof(double)
                    && type != typeof(bool)
                    && type.GetCustomAttribute<ClrScriptTypeAttribute>() == null)
                {
                    var foundInterface = false;
                    foreach (var inferfaceT in type.GetInterfaces())
                    {
                        var clrScriptTypeAtrib = inferfaceT.GetCustomAttribute<ClrScriptTypeAttribute>();

                        if (clrScriptTypeAtrib != null)
                        {
                            populateMembersFrom(inferfaceT, false, membersByName);
                            foundInterface = true;
                        }
                    }

                    if (!foundInterface)
                    {
                        throw new ClrScriptInteropException
                            ($"'{type}' cannot be used. ClrScriptTypeAttribute is missing and could not be found in an implementing interface.");
                    }
                }
                else
                {
                    populateMembersFrom(type, false, membersByName);
                }
            }

            var typeInfo = new TypeInfo(type, membersByName);
            typeInfo.OverlayExtensions(_registeredExtensions);

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

        void populateMembersFrom(Type type, bool isExtension, Dictionary<string, MemberInfo> membersByName)
        {
            foreach (var member in type.GetMembers())
            {
                var memberAtrib = member.GetCustomAttribute<ClrScriptMemberAttribute>();

                if (memberAtrib == null)
                {
                    continue;
                }

                var realName = memberAtrib.GetMemberName(member.Name);

                if (member is PropertyInfo prop)
                {
                    if (isExtension)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{prop.Name}' properties" +
                            $" cannot be used in ClrScript extension classes.");
                    }

                    if (prop.GetIndexParameters().Length > 0)
                    {
                        var haveOtherIndexers = membersByName.Values
                            .Any(m => m is PropertyInfo otherProp 
                                && otherProp.GetIndexParameters().Length > 0);

                        if (haveOtherIndexers)
                        {
                            throw new ClrScriptInteropException($"'{type}' -> '{prop.Name}' multiple indexers marked with" +
                                $" ClrScriptMemberAttribute are not supported.");
                        }
                    }

                    ValidateType(prop.PropertyType);
                }
                else if (member is FieldInfo field)
                {
                    if (isExtension)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{field.Name}' fields" +
                            $" cannot be used in ClrScript extension classes.");
                    }

                    if (field.IsStatic)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{field.Name}' cannot be used as" +
                            $" a ClrScript field. Static fields are not supported.");
                    }

                    ValidateType(field.FieldType);
                }
                else if (member is MethodInfo method)
                {
                    var parameters = method.GetParameters();

                    if (method.IsGenericMethod)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                            $" a ClrScript method because generics are not supported.");
                    }

                    if (!method.IsPublic)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                            $" a ClrScript method. ClrScript methods must be public.");
                    }

                    if (type.IsClass && method.IsAbstract)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                            $" a ClrScript method. ClrScript methods cannot be abstract.");
                    }

                    if (method.IsConstructor)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                            $" a ClrScript method. ClrScript methods cannot be constructors.");
                    }

                    if (method.IsStatic)
                    {
                        if (!isExtension)
                        {
                            throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                                $" a ClrScript method. Static methods are not supported unless used as a ClrScript extension.");
                        }
                        else
                        {
                            if (parameters.Length == 0)
                            {
                                throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                                    $" a ClrScript extension method. An extension method must have at least one parameter.");
                            }
                        }
                    }
                    else if (isExtension)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                            $" a ClrScript extension method. Method must be static.");
                    }

                    if (method.ReturnType != typeof(void))
                    {
                        ValidateType(method.ReturnType);
                    }

                    foreach (var paramInfo in parameters)
                    {
                        ValidateType(paramInfo.ParameterType);
                    }
                }

                membersByName[realName] = member;
            }
        }
    }
}
