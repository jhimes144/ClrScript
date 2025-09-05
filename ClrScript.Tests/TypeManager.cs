using ClrScript.Interop;
using ClrScript.TypeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests
{
    public class SelfReferencingType
    {
        public SelfReferencingType Reference { get; set; } = new();
    }

    [TestClass]
    public class TypeManager
    {
        [TestMethod]
        public void Self_Referencing_Type()
        {
            var manager = new TypeManagement.TypeManager();
            manager.ValidateType(typeof(SelfReferencingType));
        }
    }
}
