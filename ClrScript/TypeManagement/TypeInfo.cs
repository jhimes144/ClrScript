using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.TypeManagement
{
    public class TypeInfo
    {
        readonly IReadOnlyDictionary<string, MemberInfo> _membersByName;

        public Type Type { get; }

        internal TypeInfo(TypeManager manager, Type type)
        {
            Type = type;
            var membersByName = new Dictionary<string, MemberInfo>();

            foreach (var member in type.GetMembers())
            {
                var atrib = member.GetCustomAttribute<ClrScriptMemberAttribute>();

                if (atrib == null)
                {
                    continue;
                }

                if (member is PropertyInfo prop)
                {
                    manager.ValidateType(prop.PropertyType);
                }
                else if (member is FieldInfo field)
                {
                    if (field.IsStatic)
                    {
                        throw new ClrScriptInteropException($"'{type}' -> '{field.Name}' cannot be used as" +
                            $" a ClrScript field. Static fields are not supported.");
                    }

                    manager.ValidateType(field.FieldType);
                }
                else if (member is MethodInfo method)
                {
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

                    if (method.ReturnType != typeof(void))
                    {
                        manager.ValidateType(method.ReturnType);
                    }

                    foreach (var paramInfo in method.GetParameters())
                    {
                        manager.ValidateType(paramInfo.ParameterType);
                    }
                }

                var realName = getMemberName(member.Name, atrib.NameOverride, atrib.ConvertToCamelCase);
                membersByName[realName] = member;
            }

            _membersByName = membersByName;
        }

        public MemberInfo GetMember(string name)
        {
            return _membersByName.GetValueOrDefault(name);
        }

        static string getMemberName(string memberName, string nameOverride, bool convertToCamel)
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
