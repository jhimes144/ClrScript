using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClrScript.Visitation.Compilation
{
    class ClrScriptRoot : ClrMethodEnvironment
    {
        public ClrScriptRoot(CompilationContext context) : base(context)
        {
            Builder = context.RootClrScriptTypeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(object), new Type[] { context.ExternalTypes.InType.ClrType });

            Generator = new ILGeneratorWrapper(Builder.GetILGenerator());
        }
    }
}
