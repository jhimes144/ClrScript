using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clank.Visitation.Compilation
{
    class ClankRoot : ClrMethodEnvironment
    {
        public ClankRoot(CompilationContext context) : base(context)
        {
            Builder = context.RootClankTypeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Virtual,
                context.OutType, new Type[] { context.InType });

            Generator = new ILGeneratorWrapper(Builder.GetILGenerator());
        }
    }
}
