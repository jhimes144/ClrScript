using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests.E2E
{
    [TestClass]
    public class ObjectLiterals
    {
        [TestMethod]
        public void Basic_Assignment()
        {
            var code = @"
                var object = {
                   name: ""Bob"",
                   age: 32
                };

                object.name = ""Chris"";
                return object.name;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual("Chris", result);
        }
    }
}
