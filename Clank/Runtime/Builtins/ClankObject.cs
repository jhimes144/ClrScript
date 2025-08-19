using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Clank.Runtime.Builtins
{
    public class ClankObject
    {
        readonly Dictionary<string, object> _props 
            = new Dictionary<string, object>();

#if NET9_0
        readonly Lock _lock = new();
#else
        readonly object _lock = new object();
#endif

        public ClankObject()
        {

        }

        // this is called manually by language emit code
        public object Get(string key)
        {
            lock (_lock)
            {
                return _props.GetValueOrDefault(key);
            }
        }

        // this is called manually by language emit code
        public void Set(string key, object value)
        {
            lock (_lock)
            {
                _props[key] = value;
            }
        }

        [ClankMethod]
        [ClankNameToCamel]
        public ClankArray GetKeys()
        {
            throw new NotImplementedException();
        }

        [ClankMethod]
        [ClankNameToCamel]
        public ClankArray GetValues()
        {
            throw new NotImplementedException();
        }
    }
}
