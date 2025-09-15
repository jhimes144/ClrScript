using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.TypeManagement
{
    public class TypeInfo
    {
        readonly Dictionary<string, MemberInfo> _membersByName;

        PropertyInfo _indexerProp;

        public Type Type { get; }

        internal TypeInfo(Type type, Dictionary<string, MemberInfo> membersByName)
        {
            Type = type;
            _membersByName = membersByName;
        }

        public MemberInfo GetMember(string name)
        {
            return _membersByName.GetValueOrDefault(name);
        }

        public PropertyInfo GetIndexer()
        {
            if (_indexerProp != null)
            {
                return _indexerProp;
            }

            // we only allow one indexer property

            _indexerProp = _membersByName.Values
                .FirstOrDefault(m => m is PropertyInfo prop 
                    && prop.GetIndexParameters().Length > 0) as PropertyInfo;

            return _indexerProp;
        }

        public void OverlayExtensions(IReadOnlyList<MethodInfo> extensions)
        {
            foreach (var extensionMethod in extensions)
            {
                var parameters = extensionMethod.GetParameters();
                var memberAtrib = extensionMethod.GetCustomAttribute<ClrScriptMemberAttribute>();

                if (parameters[0].ParameterType.IsAssignableFrom(Type))
                {
                    _membersByName[memberAtrib.GetMemberName(extensionMethod.Name)] = extensionMethod;
                }
            }
        }
    }
}
