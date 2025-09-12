using ClrScript.Runtime.Builtins;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Assert.AreEqual(12, array[0]);
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

            if (result is ClrScriptArray<ClrScriptArray<double>> array)
            {
                Assert.AreEqual(4d, array.Count());
                Assert.AreEqual(1d, array[0].Count());
                Assert.AreEqual(2d, array[1].Count());
                Assert.AreEqual(3d, array[2].Count());
                Assert.AreEqual(0d, array[3].Count());

                Assert.AreEqual(1d, array[0][0]);
                Assert.AreEqual(2d, array[1][0]);
                Assert.AreEqual(3d, array[1][1]);
                Assert.AreEqual(4d, array[2][0]);
                Assert.AreEqual(5d, array[2][1]);
                Assert.AreEqual(6d, array[2][2]);
            }
            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<ClrScriptArray<double>>));
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

            if (result is ClrScriptArray<ClrScriptArray<object>> array)
            {
                Assert.AreEqual(4d, array.Count());
                Assert.AreEqual(1d, array[0].Count());
                Assert.AreEqual(1d, array[1].Count());
                Assert.AreEqual(1d, array[2].Count());
                Assert.AreEqual(1d, array[3].Count());

                Assert.AreEqual(42d, array[0][0]);
                Assert.AreEqual("hello", array[1][0]);
                Assert.AreEqual(true, array[2][0]);
                Assert.AreEqual(3.14d, array[3][0]);
            }

            Assert.IsInstanceOfType(result, typeof(ClrScriptArray<ClrScriptArray<object>>));
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
