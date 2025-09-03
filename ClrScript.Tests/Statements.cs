using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests
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

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(55d, result);
        }

        [TestMethod]
        public void For_Loop_Sum_UnOptimized()
        {
            // this will not perform any value
            // if we ever start optimizing if (false) code away.

            var code = @"
                var sum = 0;

                if (false)
                {
                   sum = ""potato"";
                }
                
                for (var i = 1; i <= 10; i = i + 1) {
                    sum = sum + i;
                }

                return sum;
            ";

            var context = ClrScriptContext<object>.Compile(code);
            var result = context.Run();
            Assert.AreEqual(55d, result);
        }
    }
}
