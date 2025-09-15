using ClrScript.Runtime.Builtins;
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
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Basic_Assignment_Auto_Prop_Name()
        {
            var code = @"
                var name = ""Bob"";
                var age = 32;

                var object = {
                   name,
                   age
                };

                return object;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();

            var obj = (ClrScriptObject)result;

            Assert.AreEqual("Bob", obj.DynGet("name"));
            Assert.AreEqual(32d, obj.DynGet("age"));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
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
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
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
            Assert.AreEqual(true, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Creation_Unknown_Shapes_Null()
        {
            var code = @"
                
                var test;

                if (false)
                {
                    test = 12;
                }
                else
                {
                    test = null;
                }

                var object = {
                   name: test,
                   age: 32
                };

                return object.name;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(null, result);
            Assert.AreEqual(true, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Missing_Property_Evaluates_Null()
        {
            var code = @"
                var object = {
                   name: ""Tom"",
                   age: 32
                };

                return object.occupation;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(null, result);
            Assert.AreEqual(true, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Property_Evaluates_Null()
        {
            var code = @"
                var object = {
                   name: ""Tom"",
                   age: null
                };

                return object.age;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(null, result);
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        // test for null asign
    }
}
