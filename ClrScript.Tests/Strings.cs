using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests
{
    [TestClass]
    public class Strings
    {
        [TestMethod]
        public void Call_Get_Length_Literal()
        {
            var code = @"
                return ""hello"".length();
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(5d, result);
            Assert.IsFalse(context.DynamicOperationsEmitted);
        }
    }
}
