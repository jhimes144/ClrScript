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
        public ClankRoot(TypeBuilder clankType, Type inType, Type outType)
        {
            Builder = clankType.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Virtual,
                outType, new Type[] { inType });

            Generator = new ILGeneratorWrapper(Builder.GetILGenerator());
        }
    }
}
