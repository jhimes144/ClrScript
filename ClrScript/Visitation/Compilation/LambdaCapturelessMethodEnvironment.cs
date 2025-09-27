using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Compilation
{
    class LambdaCapturelessMethodEnvironment : ClrMethodEnvironment
    {
        public override FieldInfo InField { get; protected set; }
        public override FieldInfo TypeManagerField { get; protected set; }

        public LambdaCapturelessMethodEnvironment(MethodBuilder methodBuilder, CompilationContext context) : base(context)
        {
            InField = context.Root.InField;
            TypeManagerField = context.Root.TypeManagerField;

            Generator = new ILGeneratorWrapper(methodBuilder.GetILGenerator());
        }
    }
}
