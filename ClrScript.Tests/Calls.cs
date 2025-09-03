using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests;

public interface ITestCallClass : IImplementsPrintStmt
{
    [ClrScriptMember(ConvertToCamelCase = true)]
    string EchoString(string value);

    [ClrScriptMember(ConvertToCamelCase = true)]
    void VoidWithInt(int value);

    [ClrScriptMember(ConvertToCamelCase = true)]
    void Void();
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
    }
}
