using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Runtime.Builtins
{
    public class ClrScriptObject
    {
        readonly Dictionary<string, object> _dynProperties
            = new Dictionary<string, object>();

        public void DynSet(string key, object value)
        {
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
