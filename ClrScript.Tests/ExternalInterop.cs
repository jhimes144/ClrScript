using Mono.Cecil.Cil;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests
{
    [ClrScriptType]
    public interface ITestInteropClass : IImplementsPrintStmt
    {
        [ClrScriptMember(ConvertToCamelCase = true)]
        string RootStringProp { get; set; }

        [ClrScriptMember(ConvertToCamelCase = true)]
        int RootIntProp { get; set; }

        [ClrScriptMember(ConvertToCamelCase = true)]
        bool RootBoolProp { get; set; }
    }

    [TestClass]
    public class ExternalInterop
    {
        class InternalTestClass { }

        [TestMethod]
        public void InternalClassFails()
        {
            Assert.ThrowsException<ClrScriptInteropException>
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
            Assert.AreEqual("hello world", printValue);
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
            Assert.AreEqual("hello world", testClass.Object.RootStringProp);
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
            Assert.AreEqual("hello world", testClass.Object.RootStringProp);
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
            Assert.AreEqual(true, testClass.Object.RootBoolProp);
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
            Assert.AreEqual(12, testClass.Object.RootIntProp);
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
            Assert.AreEqual(12, testClass.Object.RootIntProp);
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
    }
}
