using ClrScript.Runtime.Builtins;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests;

[ClrScriptType]
public interface ITestCallClass : IImplementsPrintStmt
{
    [ClrScriptMember(ConvertToCamelCase = true)]
    string EchoString(string value);

    [ClrScriptMember(ConvertToCamelCase = true)]
    void VoidWithInt(int value);

    [ClrScriptMember(ConvertToCamelCase = true)]
    void Void();

    [ClrScriptMember(ConvertToCamelCase = true)]
    int ReturnsInt();
}

[TestClass]
public class Calls
{
    [TestMethod]
    public void External_Call_EchoString()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                return echoString(""hello world"");
            ");

        var result = context.Run(testClass.Object);
        Assert.AreEqual("hello world", result);
        Assert.AreEqual(false, context.DynamicOperationsEmitted);
    }

    // TODO: Test to make sure you cannot assign root members

    [TestMethod]
    public void External_Call_EchoString_No_Return_Handle()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                echoString(""hello world"");
                return ""hello"";
            ");

        var result = context.Run(testClass.Object);
        Assert.AreEqual("hello", result);
        Assert.AreEqual(false, context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void External_Call_EchoString_Void_To_Null_Return()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);
        testClass.Setup(m => m.Void()).Callback(() => { });

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                return echoString(void());
            ");

        var result = context.Run(testClass.Object);
        Assert.AreEqual(null, result);
        Assert.AreEqual(true, context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void External_Call_EchoString_Bad_Argument()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                return echoString(12);
            ");

        Assert.ThrowsException<ClrScriptRuntimeException>(() =>
        {
            context.Run(testClass.Object);
        });
    }

    [TestMethod]
    public void External_Call_EchoString_Bad_Argument_2()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                return echoString({});
            ");

        Assert.ThrowsException<ClrScriptRuntimeException>(() =>
        {
            context.Run(testClass.Object);
        });
    }

    [TestMethod]
    public void External_Call_EchoString_Too_Many_Args()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                return echoString(""hello world"", ""hello world"");
            ");

        Assert.ThrowsException<ClrScriptRuntimeException>(() =>
        {
            context.Run(testClass.Object);
        });
    }

    [TestMethod]
    public void External_Call_EchoString_Too_Few_Args()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                return echoString();
            ");

        Assert.ThrowsException<ClrScriptRuntimeException>(() =>
        {
            context.Run(testClass.Object);
        });
    }

    [TestMethod]
    public void External_Call_Double_Arg_To_Int()
    {
        var testClass = new Mock<ITestCallClass>();
        int valuePassed = 0;
        testClass.Setup(m => m.VoidWithInt(It.IsAny<int>())).Callback((int v) => valuePassed = v);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                voidWithInt(12);
            ");

        context.Run(testClass.Object);
        Assert.AreEqual(12, valuePassed);
        Assert.AreEqual(false, context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void External_Call_EchoString_Unknown_Argument()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.EchoString(It.IsAny<string>())).Returns((string str) => str);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                var arg;

                if (false)
                {
                    arg = 12;
                }
                else
                {
                    arg = ""hello world"";
                }

                return echoString(arg);
            ");

        var result = context.Run(testClass.Object);
        Assert.AreEqual("hello world", result);
        Assert.AreEqual(true, context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void External_Call_Returning_Int_Optimized_Returns_Double()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.ReturnsInt()).Returns(() => 12);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                return returnsInt();
            ");

        var result = context.Run(testClass.Object);
        Assert.AreEqual(12d, result);
        Assert.AreEqual(false, context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void External_Call_Returning_Int_Not_Optimized_Returns_Double()
    {
        var testClass = new Mock<ITestCallClass>();
        testClass.Setup(m => m.ReturnsInt()).Returns(() => 12);

        var context = ClrScriptContext<ITestCallClass>.Compile(@"
                var t;

                if (false)
                {
                    t = ""hello"";
                }
                else
                {
                    t = returnsInt();
                }
                
                voidWithInt(t);
                return t;
            ");

        var result = context.Run(testClass.Object);
        Assert.AreEqual(12d, result);
        Assert.AreEqual(true, context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void Call_Null()
    {
        var code =
        @"
            var calculate = null;
            return calculate(12, 12);
        ";

        var context = ClrScriptContext<object>.Compile(code);
        Assert.ThrowsException<ClrScriptRuntimeException>(() => context.Run());
        Assert.IsTrue(context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void Lambda_Parameter_Assignment_Disallowed()
    {
        var code = @"
                var calculate = (x, y) -> {
                    x = 32;
                    y = 32;
                    return x + y;
                };
                
                var value = calculate(12, 12);
                return value;
            ";

        Assert.ThrowsException<ClrScriptCompileException>(() => ClrScriptContext<object>.Compile(code));
    }

    [TestMethod]
    public void Lambda_Basic()
    {
        var code = @"
                var calculate = (x, y) -> {
                    var sum = x + y;
                    var product = x * y;
                    return sum + product;
                };
                
                var value = calculate(12, 12);
                return { value, calculate };
            ";

        var context = ClrScriptContext<object>.Compile(code);
        var result = context.Run();
        Assert.IsFalse(context.DynamicOperationsEmitted);
        Assert.IsInstanceOfType(result, typeof(ClrScriptObject));
        var obj = (ClrScriptObject)result;

        Assert.AreEqual(168d, obj.DynGet("value"));
        Assert.AreEqual(typeof(Func<double, double, double>), obj.DynGet("calculate").GetType());
    }

    [TestMethod]
    public void Lambda_Basic_Nested()
    {
        var code = @"
                var calculate = (x, y) -> {
                    var getNum = () -> {
                        return 13;
                    };
                    
                    var sum = x + y;
                    return sum + getNum();
                };
                
                var value = calculate(12, 12);
                return value;
            ";

        var context = ClrScriptContext<object>.Compile(code);
        var result = context.Run();
        Assert.AreEqual(37d, result);
        Assert.IsFalse(context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void Lambda_Unknown()
    {
        var code =
        @"
            var calculate;

            if (false)
            {
                calculate = ""hello"";
            }
            else
            {
                calculate = (x, y) -> {
                    var sum = x + y;
                    var product = x * y;
                    return sum + product;
                };
            }
            
            return calculate(12, 12);
        ";

        var context = ClrScriptContext<object>.Compile(code);
        var result = context.Run();
        Assert.IsTrue(context.DynamicOperationsEmitted);
        Assert.AreEqual(168d, result);
    }

    [TestMethod]
    public void Lambda_Unknown_Args_All()
    {
        var code =
        @"
            var calculate = (x, y) -> {
                    return x + y;
                };
            
            var test1 = calculate(12, 12);
            var test2 = calculate(""hello"", "" world"");

            return { test1, test2, calculate };
        ";

        var context = ClrScriptContext<object>.Compile(code);
        var result = context.Run();

        Assert.IsInstanceOfType(result, typeof(ClrScriptObject));
        var obj = (ClrScriptObject)result;

        Assert.IsInstanceOfType(obj.DynGet("calculate"), typeof(Func<object, object, object>));
        Assert.AreEqual(24d, obj.DynGet("test1"));
        Assert.AreEqual("hello world", obj.DynGet("test2"));

        Assert.IsTrue(context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void Lambda_Dyn_Stowaway()
    {
        var code =
        @"
             var calculate = (x, y) -> {
                     return x + y;
                 };
             
             var test1 = calculate(12, 12);
             var calcDyn;

             if (true)
             {
                 calcDyn = calculate;
             }
             else
             {
                 calcDyn = 12;
             }

             return calcDyn(""hello"", "" world""); 
        ";

        var context = ClrScriptContext<object>.Compile(code);
        var result = context.Run();

        Assert.AreEqual("hello world", result);
        Assert.IsTrue(context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void Lambda_Unknown_Args_Binary_Mid_Point()
    {
        var code =
            @"
                var calculate = (x, y) -> {
                    return x + y;
                };

                var test1 = calculate(1, 1);
                var test2 = calculate(1, 1);
                var sumTest = test1 + test2;
                var sumSumTest = sumTest + 1;
                
                var test3 = calculate(""hello"", "" world"");

                return { test1, test2, sumSumTest, test3, calculate };
            ";

        var context = ClrScriptContext<object>.Compile(code);
        var result = context.Run();

        Assert.IsInstanceOfType(result, typeof(ClrScriptObject));
        var obj = (ClrScriptObject)result;

        Assert.IsInstanceOfType(obj.DynGet("calculate"), typeof(Func<object, object, object>));
        Assert.AreEqual(2d, obj.DynGet("test1"));
        Assert.AreEqual(2d, obj.DynGet("test2"));
        Assert.AreEqual(5d, obj.DynGet("sumSumTest"));
        Assert.AreEqual("hello world", obj.DynGet("test3"));

        Assert.IsTrue(context.DynamicOperationsEmitted);
    }

    [TestMethod]
    public void Lambda_Basic_Var_Capture()
    {
        var code =
        @"
             var val = 12;

             var calculate = (x) -> {
                     return x + val;
                 };

             return calculate(32); 
        ";

        var context = ClrScriptContext<object>.Compile(code);
        var result = context.Run();

        Assert.AreEqual(44d, result);
        Assert.IsFalse(context.DynamicOperationsEmitted);
    }
}
