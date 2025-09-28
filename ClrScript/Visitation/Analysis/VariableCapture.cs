using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Analysis
{
    abstract class LambdaCapture { }

    class VariableCapture : LambdaCapture
    {
        public string Name { get; }

        public VariableCapture(string name)
        {
            Name = name;
        }
    }

    class OuterLambdaArgCapture { }
}
