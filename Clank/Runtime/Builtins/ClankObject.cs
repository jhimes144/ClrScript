using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Runtime.Builtins
{
    public class ClankObject
    {
        readonly Dictionary<string, object> _properties
            = new Dictionary<string, object>();

        public void Set(string key, object value)
        {
            _properties[key] = value;
        }

        public object Get(string key)
        {
            return _properties.GetValueOrDefault(key);
        }
    }
}
