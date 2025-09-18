using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Compilation
{
    class LambdaMethodEnvironment : ClrMethodEnvironment
    {
        public LambdaMethodEnvironment(DynamicMethod dynMethod, CompilationContext context) : base(context)
        {
            Generator = new ILGeneratorWrapper(dynMethod.GetILGenerator());
        }
    }
}
