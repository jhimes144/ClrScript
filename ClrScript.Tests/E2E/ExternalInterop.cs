using Mono.Cecil.Cil;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests.E2E
{
    public interface ITestInteropClass : IImplementsPrintStmt
    {
        [ClrScriptProperty(ConvertToCamelCase = true)]
        string RootStringProp { get; set; }
    }

    [TestClass]
    public class ExternalInterop
    {
        class InternalTestClass { }

        [TestMethod]
        public void InternalClassFails()
        {
            Assert.ThrowsException<ClrScriptCompileException>
                (() => ClrScriptContext<InternalTestClass>.Compile("var t = 12;"));
        }

        [TestMethod]
        public void Basic_Prop_Get()
        {
            var testClass = new Mock<ITestInteropClass>();
            object printValue = new();

            testClass.Setup(m => m.Print(It.IsAny<object>())).Callback((object obj) => printValue = obj);
            testClass.SetupGet(m => m.RootStringProp).Returns("hello world");

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                print rootStringProp;
            ");

            context.Run(testClass.Object);
            Assert.AreEqual(printValue, "hello world");
        }

        [TestMethod]
        public void Basic_Prop_Set()
        {
            var testClass = new Mock<ITestInteropClass>();

            testClass.SetupAllProperties();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                rootStringProp = ""hello world"";
            ");

            context.Run(testClass.Object);
            Assert.AreEqual(testClass.Object.RootStringProp, "hello world");
        }

        [TestMethod]
        public void Basic_Prop_Set_Wrong_Type()
        {
            var testClass = new Mock<ITestInteropClass>();

            testClass.SetupAllProperties();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                rootStringProp = 12;
            ");

            context.Run(testClass.Object);
            Assert.AreEqual(testClass.Object.RootStringProp, "hello world");
        }
    }
}
