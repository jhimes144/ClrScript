using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Tests.Grammer;

[TestClass]
public class Identifers
{
    [TestMethod]
    public void Start_With_Number()
    {
        var context = ClrScriptContext<object>.Compile(@"
            var 4array1 = [];
        ");
    }

}
