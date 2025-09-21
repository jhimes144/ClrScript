using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Runtime.Builtins
{
    /// <summary>
    /// Internal representation of objects in ClrScript.
    /// </summary>
    public class ClrScriptObject
    {
        readonly Dictionary<string, object> _dynProperties
            = new Dictionary<string, object>();

        // mind the name, generated code in shape table relies on this member and its name being the same
        readonly protected bool _hasFields;

        public bool HasDynamicProperties()
        {
            return _dynProperties.Count > 0;
        }

        public void DynSet(string key, object value)         
        {
            if (!_hasFields)
            {
                _dynProperties[key] = value;
                return;
            }

            var type = GetType();
            var field = type.GetField(key);

            if (field != null)
            {
                if (field.FieldType.IsAssignableFrom(value.GetType()))
                {
                    field.SetValue(this, value);
                }
                else
                {
                    field.SetValue(this, null);
                    _dynProperties[key] = value;
                }
            }
            else
            {
                _dynProperties[key] = value;
            }
        }

        public object DynGet(string key)
        {
            if (_dynProperties.TryGetValue(key, out var value))
            {
                return value;
            }

            if (!_hasFields)
            {
                return null;
            }

            var type = GetType();
            var field = type.GetField(key);

            if (field != null)
            {
                return field.GetValue(this);
            }

            return null;
        }
    }
}
