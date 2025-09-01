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
        [ClrScriptMember(ConvertToCamelCase = true)]
        string RootStringProp { get; set; }

        [ClrScriptMember(ConvertToCamelCase = true)]
        int RootIntProp { get; set; }

        [ClrScriptMember(ConvertToCamelCase = true)]
        bool RootBoolProp { get; set; }

        [ClrScriptMember(ConvertToCamelCase = true)]
        public string EchoString(string value)
        {
            return value;
        }
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
        public void Basic_Prop_Set_String_Unoptimized()
        {
            var testClass = new Mock<ITestInteropClass>();

            testClass.SetupAllProperties();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                var value = ""hello world"";
            
                if (false)
                {
                    value = 12;
                }

                rootStringProp = value;
            ");

            context.Run(testClass.Object);
            Assert.AreEqual(testClass.Object.RootStringProp, "hello world");
        }

        [TestMethod]
        public void Basic_Prop_Set_Bool_Unoptimized()
        {
            var testClass = new Mock<ITestInteropClass>();

            testClass.SetupAllProperties();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                var value = true;
            
                if (false)
                {
                    value = 12;
                }

                rootBoolProp = value;
            ");

            context.Run(testClass.Object);
            Assert.AreEqual(testClass.Object.RootBoolProp, true);
        }

        [TestMethod]
        public void Basic_Prop_Set_Double_To_Int()
        {
            var testClass = new Mock<ITestInteropClass>();
             testClass.SetupAllProperties();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                rootIntProp = 12;
            ");

            context.Run(testClass.Object);
            Assert.AreEqual(testClass.Object.RootIntProp, 12);
        }

        [TestMethod]
        public void Basic_Prop_Set_Double_To_Int_Unoptimized()
        {
            var testClass = new Mock<ITestInteropClass>();
            testClass.SetupAllProperties();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                var value = 12;

                if (false)
                {
                    value = ""hello world"";
                }

                rootIntProp = value;
            ");

            context.Run(testClass.Object);
            Assert.AreEqual(testClass.Object.RootIntProp, 12);
        }

        [TestMethod]
        public void Basic_Prop_Set_Wrong_Type()
        {
            var testClass = new Mock<ITestInteropClass>();

            testClass.SetupAllProperties();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                rootStringProp = 12;
            ");

            Assert.ThrowsException<ClrScriptRuntimeException>
                (() => context.Run(testClass.Object));
        }

        [TestMethod]
        public void Basic_External_Call()
        {
            var testClass = new Mock<ITestInteropClass>();

            var context = ClrScriptContext<ITestInteropClass>.Compile(@"
                return echoString(""hello world"");
            ");

            var result = context.Run(testClass.Object);
            Assert.AreEqual(result, "hello world");
        }
    }
}
