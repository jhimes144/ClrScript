using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests.E2E
{
    [TestClass]
    public class Statements
    {
        [TestMethod]
        public void For_Loop_Sum()
        {
            var code = @"
                var sum = 0;
                for (var i = 1; i <= 10; i = i + 1) {
                    sum = sum + i;
                }
                return sum;
            ";

            var context = ClrScriptContext<object, double>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(55, result);
        }
    }
}
