using ClrScript.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Compilation
{
    class ClrScriptModule : ClrMethodEnvironment
    {
        // todo: type per module

        public string Name { get; }

        public ClrScriptModule(string name, CompilationContext context) : base(context)
        {
            Name = name;
            //Builder = clankType.DefineMethod(name, MethodAttributes.Private, null, null);

            // Generator = new ILGeneratorWrapper(Builder.GetILGenerator());
        }
    }
}
