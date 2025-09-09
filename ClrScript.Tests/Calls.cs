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
}
