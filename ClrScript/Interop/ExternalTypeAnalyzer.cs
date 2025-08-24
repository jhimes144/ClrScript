using ClrScript.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ClrScript.Interop
{
    class ExternalTypeAnalyzer
    {
        readonly Dictionary<string, ExternalType> _externalTypesByRealTypeName 
            = new Dictionary<string, ExternalType>();

        public IReadOnlyDictionary<string, ExternalType> ExternalTypesByRealTypeName => _externalTypesByRealTypeName;

        public ExternalType InType { get; private set; }

        public MethodInfo PrintStmtMethod { get; private set; }

        // TODO: Need to be able to throw exception when a type has the same name from a different namespace
        // since ClrScript does not support namespaces.

        public ExternalTypeAnalyzer(ClrScriptCompilationSettings settings)
        {
        }

        public void SetInType(Type type)
        {
            Analyze(type);
            InType = _externalTypesByRealTypeName[type.Name];

            if (typeof(IImplementsPrintStmt).IsAssignableFrom(InType.ClrType))
            {
                PrintStmtMethod = typeof(IImplementsPrintStmt).GetMethod
                    (nameof(IImplementsPrintStmt.Print), new Type[] { typeof(object) });
            }
        }

        public void Analyze(Type type)
        {
            // TODO: This will cause a stack overflow for self referencing types. i.e
            // class Person
            // {
            //   Person person;
            // }

            if (type.IsPrimitive)
            {
                return;
            }

            if (type == typeof(string))
            {
                return;
            }

            if (_externalTypesByRealTypeName.ContainsKey(type.Name))
            {
                return;
            }

            //if (type.IsValueType && type != 
            //    _numberType && type != typeof(bool))
            //{
            //    throw new ClrScriptInteropException($"'{type}' is an invalid value type. It must be either a {_numberType.Name} or boolean.");
            //}

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

            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttribute<ClrScriptMethodAttribute>() != null);

            var methodResults = new List<ExternalTypeMethod>();

            foreach (var method in methods)
            {
                var atrib = method.GetCustomAttribute<ClrScriptMethodAttribute>();
                var name = getMemberName(method.Name, atrib.NameOverride, atrib.ConvertToCamelCase);

                if (method.IsGenericMethod)
                {
                    throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                        $" a ClrScript method because generics are not supported.");
                }

                if (method.IsStatic)
                {
                    throw new ClrScriptInteropException($"'{type}' -> '{method.Name}' cannot be used as" +
                        $" a ClrScript method. Static methods are not supported.");
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
                .Where(m => m.GetCustomAttribute<ClrScriptPropertyAttribute>() != null);

            var propResults = new List<ExternalTypeProperty>();

            foreach (var prop in props)
            {
                var atrib = prop.GetCustomAttribute<ClrScriptPropertyAttribute>();
                var name = getMemberName(prop.Name, atrib.NameOverride, atrib.ConvertToCamelCase);

                Analyze(prop.PropertyType);

                propResults.Add(new ExternalTypeProperty
                {
                    NameOverride = name,
                    Property = prop
                });
            }

            var fields = type.GetFields()
                .Where(f => f.GetCustomAttribute<ClrScriptFieldAttribute>() != null);

            var fieldResults = new List<ExternalTypeField>();

            foreach (var field in fields)
            {
                var atrib = field.GetCustomAttribute<ClrScriptFieldAttribute>();
                var name = getMemberName(field.Name, atrib.NameOverride, atrib.ConvertToCamelCase);

                if (field.IsStatic)
                {
                    throw new ClrScriptInteropException($"'{type}' -> '{field.Name}' cannot be used as" +
                        $" a ClrScript field. Static fields are not supported.");
                }

                Analyze(field.FieldType);

                fieldResults.Add(new ExternalTypeField
                {
                    NameOverride = name,
                    Field = field
                });
            }

            _externalTypesByRealTypeName[type.Name] 
                = new ExternalType(type.Name, type, methodResults, propResults, fieldResults);
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
