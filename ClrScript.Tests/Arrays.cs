using ClrScript.Runtime.Builtins;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClrScript.Tests
{
    [TestClass]
    public class Arrays
    {
        [TestMethod]
        public void Array_Literal_Empty()
        {
            var context = ClrScriptContext<object>.Compile(@"
                return [];
            ");

            var result = context.Run();

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Empty_Literal_Numbers_Inferred()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var array = [];
                array.add(12);
                return array;
            ");

            var result = context.Run();

            if (result is ClrScriptArray<double> array)
            {
                Assert.AreEqual(1d, array.Count());
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<double>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Bad_Literal_1()
        {
            Assert.ThrowsException<ClrScriptCompileException>(() =>
            {
                ClrScriptContext<object>.Compile(@"
                    return [12,,];
                ");
            });
        }


        [TestMethod]
        public void Array_Bad_Literal_2()
        {
            Assert.ThrowsException<ClrScriptCompileException>(() =>
            {
                ClrScriptContext<object>.Compile(@"
                    return [12,,13];
                ");
            });
        }

        [TestMethod]
        public void Array_Bad_Literal_3()
        {
            Assert.ThrowsException<ClrScriptCompileException>(() =>
            {
                ClrScriptContext<object>.Compile(@"
                    return [,12];
                ");
            });
        }

        [TestMethod]
        public void Array_Literal_Numbers()
        {
            var context = ClrScriptContext<object>.Compile(@"
                return [12, 32.2, 45];
            ");

            var result = context.Run();

            if (result is ClrScriptArray<double> array)
            {
                Assert.AreEqual(3d, array.Count());
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<double>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Literal_Bools()
        {
            var context = ClrScriptContext<object>.Compile(@"
                return [true, false, true];
            ");

            var result = context.Run();

            if (result is ClrScriptArray<bool> array)
            {
                Assert.AreEqual(3d, array.Count());
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<bool>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Literal_Numbers_With_Add()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var array = [12, 32.2, 45];
                array.add(89);

                return array;
            ");

            var result = context.Run();

            if (result is ClrScriptArray<double> array)
            {
                Assert.AreEqual(4d, array.Count());
                Assert.AreEqual(89d, array[3]);
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<double>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Literal_Add_Differing_Type()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var array = [12, 32.2, 45];
                array.add(""hello"");

                return array;
            ");

            var result = context.Run();

            if (result is ClrScriptArray<object> array)
            {
                Assert.AreEqual(4d, array.Count());
                Assert.AreEqual(12d, array[0]);
                Assert.AreEqual("hello", array[3]);
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Literal_Add_Differing_Type_Reference_Assignments()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var array = [12, 32.2, 45];
                array.add(97);

                if (false)
                {
                    array.add(22);
                }
                else
                {
                    var t = array;
                    t.add(""hello"");
                }

                return array;
            ");

            var result = context.Run();

            if (result is ClrScriptArray<object> array)
            {
                Assert.AreEqual(5d, array.Count());
                Assert.AreEqual(12d, array[0]);
                Assert.AreEqual(97d, array[3]);
                Assert.AreEqual("hello", array[4]);
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Reference_Type_Inference()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var array1 = [12, 32.2, 45];
                var array2 = array1;
                array2.add(""hello"");
                return array2;
            ");

            var result = context.Run();

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));

            var array = (ClrScriptArray<object>)result;
            Assert.AreEqual(4d, array.Count());
            Assert.AreEqual(12d, array[0]);
            Assert.AreEqual(32.2d, array[1]);
            Assert.AreEqual(45d, array[2]);
            Assert.AreEqual("hello", array[3]);
        }

        [TestMethod]
        public void Array_Complex_Type_Inference()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var obj = {
                    name: ""Tim"",
                    places: [""Alaska"", ""New York""],
                    favNumbers: []
                };

                var objAnother = {
                    name: ""Phil"",
                    places: [""Alaska""],
                    age: 55
                };

                var container = [obj, objAnother];

                obj.favNumbers.add(12);

                var obj2 = {
                    array: container
                };

                obj2.array[0].places.add(""China"");

                return obj2;
            ");

            var result = context.Run();

            Assert.IsNotInstanceOfType(result, typeof(ClrScriptArray<object>));

            var resultArray = (IList)((ClrScriptObject)result).DynGet("array");
            var typesSame = resultArray[0].GetType() == resultArray[1].GetType();
            var objType = resultArray[0].GetType();

            Assert.IsTrue(objType.GetField("name").FieldType == typeof(string));
            Assert.IsTrue(objType.GetField("places").FieldType == typeof(ClrScriptArray<string>));
            Assert.IsTrue(objType.GetField("favNumbers").FieldType == typeof(ClrScriptArray<double>));
            Assert.IsTrue(objType.GetField("age").FieldType == typeof(double));

            Assert.IsTrue(typesSame);
            Assert.IsFalse(context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Literal_Add_Differing_Indexer()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var array = [12, 32.2, 45, 22];
                array[3] = ""hello"";

                return array;
            ");

            var result = context.Run();

            if (result is ClrScriptArray<object> array)
            {
                Assert.AreEqual(4d, array.Count());
                Assert.AreEqual(12d, array[0]);
                Assert.AreEqual("hello", array[3]);
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Indexer()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var array = [12, 92, 45];
                return array[1];
            ");

            var result = context.Run();

            Assert.AreEqual(92d, result);
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Literal_Of_Arrays_Of_Numbers()
        {
            var context = ClrScriptContext<object>.Compile(@"
                return [[12, 32.2, 45]];
            ");

            var result = context.Run();

            if (result is ClrScriptArray<ClrScriptArray<double>> array)
            {
                Assert.AreEqual(1d, array.Count());
                Assert.IsInstanceOfType(array[0], typeof(ClrScriptArray<double>));
                Assert.AreEqual(3d, array[0].Count());
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<ClrScriptArray<double>>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Of_Arrays_Different_Lengths()
        {
            var context = ClrScriptContext<object>.Compile(@"
                return [[1], [2, 3], [4, 5, 6], []];
            ");

            var result = context.Run();

            if (result is ClrScriptArray<object> array)
            {
                Assert.AreEqual(4d, array.Count());
                Assert.IsInstanceOfType(array[0], typeof(ClrScriptArray<double>));
                Assert.IsInstanceOfType(array[1], typeof(ClrScriptArray<double>));
                Assert.IsInstanceOfType(array[2], typeof(ClrScriptArray<double>));
                Assert.IsInstanceOfType(array[3], typeof(ClrScriptArray<object>));
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Of_Arrays_Matrix_Addition()
        {
            var context = ClrScriptContext<object>.Compile(@"
                var matrix = [[1, 2], [3, 4]];
                return matrix[0][0] + matrix[1][1];
            ");

            var result = context.Run();

            Assert.AreEqual(5d, result);
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Of_Arrays_Single_Element_Arrays()
        {
            var context = ClrScriptContext<object>.Compile(@"
                return [[42], [""hello""], [true], [3.14]];
            ");

            var result = context.Run();

            if (result is ClrScriptArray<object> array)
            {
                //Assert.AreEqual(4d, array.Count());
                //Assert.AreEqual(1d, array[0].Count());
                //Assert.AreEqual(1d, array[1].Count());
                //Assert.AreEqual(1d, array[2].Count());
                //Assert.AreEqual(1d, array[3].Count());

                //Assert.AreEqual(42d, array[0][0]);
                //Assert.AreEqual("hello", array[1][0]);
                //Assert.AreEqual(true, array[2][0]);
                //Assert.AreEqual(3.14d, array[3][0]);
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }

        [TestMethod]
        public void Array_Literal_Mix()
        {
            var context = ClrScriptContext<object>.Compile(@"
                return [12, ""hello"", 45];
            ");

            var result = context.Run();

            if (result is ClrScriptArray<object> array)
            {
                Assert.AreEqual(array.Count(), 3d);
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<object>));
            Assert.AreEqual(false, context.DynamicOperationsEmitted);
        }
    }
}
