using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Runtime
{
    public static class TestLambda
    {
        public static double Testf(double x, double y)
        {
            // Calculate the sum
            var sum = x + y;
            // Calculate the product
            var product = x * y;
            // Return the sum of both
            return sum + product;
        }
    }
}
