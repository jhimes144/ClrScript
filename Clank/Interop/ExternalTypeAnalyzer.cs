using Clank.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Clank.Interop
{
    class ExternalTypeAnalyzer
    {
        readonly Type _numberType;
        readonly Dictionary<string, ExternalType> _externalTypesByRealTypeName 
            = new Dictionary<string, ExternalType>();

        public IReadOnlyDictionary<string, ExternalType> ExternalTypesByRealTypeName => _externalTypesByRealTypeName;

        public ExternalType InType { get; private set; }

        // TODO: Need to be able to throw exception when a type has the same name from a different namespace
        // since Clank does not support namespaces.

        public ExternalTypeAnalyzer(ClankCompilationSettings settings)
        {
            _numberType = settings.NumberPrecision == NumberPrecision.DoublePrecision 
                ? typeof(double) : typeof(float);
        }

        public void SetInType(Type type)
        {
            if (type.IsValueType || !(type.IsInterface || type.IsClass))
            {
                throw new ClankCompileException($"Type {type} in an invalid IN type.");
            }

            Analyze(type);
            InType = _externalTypesByRealTypeName[type.FullName];
        }

        public ClankType GetMetaForInTypeMemberByName(string name)
        {
            return GetMetaForTypeMemberByName(InType, name);
        }

        /// <summary>
        /// This will scan type for a member with given name. If its found, will do the following:
        /// IF its a method, return type is returned.
        /// IF its a property, the property type is returned
        /// IF its a field, field type is returned.
        /// IF the member cannot be found, null is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ClankType GetMetaForTypeMemberByName(ExternalType type, string name)
        {
            var method = InType.Methods.FirstOrDefault(m => m.NameOverride == name);

            if (method != null)
            {
                var returnTypeName = method.Method.ReturnType?.Name;

                if (returnTypeName != null)
                {
                    var returnTypeExternal = _externalTypesByRealTypeName[returnTypeName];
                    return new ClankType(returnTypeName, returnTypeExternal);
                }
                else
                {
                    return ClankType.Void;
                }
            }

            var prop = InType.Properties.FirstOrDefault(m => m.NameOverride == name);

            if (prop != null)
            {
                var typeName = prop.Property.PropertyType.Name;
                var propTypeExternal = _externalTypesByRealTypeName[typeName];
                return new ClankType(typeName, type);
            }

            var field = InType.Fields.FirstOrDefault(m => m.NameOverride == name);

            if (field != null)
            {
                var typeName = field.Field.FieldType.Name;
                var fieldTypeExternal = _externalTypesByRealTypeName[typeName];
                return new ClankType(typeName, type);
            }

            return null;
        }

        public void Analyze(Type type)
        {
            // TODO: This will cause a stack overflow for self referencing types. i.e
            // class Person
            // {
            //   Person person;
            // }

            if (_externalTypesByRealTypeName.ContainsKey(type.Name))
            {
                return;
            }

            if (type.IsValueType && type != 
                _numberType && type != typeof(bool))
            {
                throw new ClankCompileException($"'{type}' is an invalid value type. It must be either a {_numberType.Name} or boolean.");
            }

            if (type.IsGenericType)
            {
                throw new ClankCompileException($"'{type}' is an invalid Clank type. Generics are not supported.");
            }

            if (type.IsPointer)
            {
                throw new ClankCompileException($"'{type}' is an invalid Clank type. Pointers are not supported.");
            }

            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttribute<ClankMethodAttribute>() != null);

            var methodResults = new List<ExternalTypeMethod>();

            foreach (var method in methods)
            {
                var atrib = method.GetCustomAttribute<ClankMethodAttribute>();
                var name = getMemberName(method.Name, atrib.NameOverride, atrib.ConvertToCamelCase);

                if (method.IsGenericMethod)
                {
                    throw new ClankCompileException($"'{type}' -> '{method.Name}' cannot be used as" +
                        $" a Clank method because generics are not supported.");
                }

                if (method.IsStatic)
                {
                    throw new ClankCompileException($"'{type}' -> '{method.Name}' cannot be used as" +
                        $" a Clank method. Static methods are not supported.");
                }

                if (method.ReturnType != null)
                {
                    Analyze(method.ReturnType);
                }

                foreach (var parameter in method.GetParameters())
                {
                    Analyze(parameter.ParameterType);
                }

                methodResults.Add(new ExternalTypeMethod
                {
                    Method = method,
                    NameOverride = name,
                });
            }

            var props = type.GetProperties()
                .Where(m => m.GetCustomAttribute<ClankPropertyAttribute>() != null);

            var propResults = new List<ExternalTypeProperty>();

            foreach (var prop in props)
            {
                var atrib = prop.GetCustomAttribute<ClankPropertyAttribute>();
                var name = getMemberName(prop.Name, atrib.NameOverride, atrib.ConvertToCamelCase);

                Analyze(prop.PropertyType);

                propResults.Add(new ExternalTypeProperty
                {
                    NameOverride = name,
                    Property = prop
                });
            }

            var fields = type.GetFields()
                .Where(f => f.GetCustomAttribute<ClankFieldAttribute>() != null);

            var fieldResults = new List<ExternalTypeField>();

            foreach (var field in fields)
            {
                var atrib = field.GetCustomAttribute<ClankFieldAttribute>();
                var name = getMemberName(field.Name, atrib.NameOverride, atrib.ConvertToCamelCase);

                if (field.IsStatic)
                {
                    throw new ClankCompileException($"'{type}' -> '{field.Name}' cannot be used as" +
                        $" a Clank field. Static fields are not supported.");
                }

                Analyze(field.FieldType);

                fieldResults.Add(new ExternalTypeField
                {
                    NameOverride = name,
                    Field = field
                });
            }

            _externalTypesByRealTypeName[type.FullName] 
                = new ExternalType(methodResults, propResults, fieldResults);
        }

        string getMemberName(string memberName, string nameOverride, bool convertToCamel)
        {
            if (convertToCamel)
            {
                memberName = Util.ConvertStrToCamel(memberName);
            }
            else if (!string.IsNullOrWhiteSpace(nameOverride))
            {
                memberName = nameOverride;
            }

            return memberName;
        }
    }
}
