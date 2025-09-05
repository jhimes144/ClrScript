using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClrScript.TypeManagement;

namespace ClrScript.Visitation.Compilation
{
    class ClrScriptRoot : ClrMethodEnvironment
    {
        public ClrScriptRoot(CompilationContext context) : base(context)
        {
            Builder = context.RootClrScriptTypeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(object), new Type[] { context.InType, typeof(TypeManager) });

            Generator = new ILGeneratorWrapper(Builder.GetILGenerator());
        }
    }
}
