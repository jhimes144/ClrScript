using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests
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

        [TestMethod]
        public void Assign_Complex()
        {
            var code = @"
                var object = {
                   name: ""Bob"",
                   age: 32
                };

                var object2 = {
                   name: ""Bob"",
                   age: 55
                };

                var container = {};

                if (false)
                {
                    container.person = object2;
                }
                else
                {
                    container.person = object;
                }

                container.person.name = ""Chris"";
                return container.person.name;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual("Chris", result);
        }

        [TestMethod]
        public void Creation_Unknown_Shapes()
        {
            var code = @"
                
                var test;

                if (false)
                {
                    test = 12;
                }
                else
                {
                    test = ""Billy"";
                }

                var object = {
                   name: test,
                   age: 32
                };

                return object.name;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual("Billy", result);
        }

        // test for null asign
    }
}
