using Clank.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.Compilation
{
    class ClankModule : ClrMethodEnvironment
    {
        // todo: type per module

        public string Name { get; }

        public ClankModule(string name, TypeBuilder clankType)
        {
            Name = name;
            Builder = clankType.DefineMethod(name, MethodAttributes.Private, null, null);

            Generator = new ILGeneratorWrapper(Builder.GetILGenerator());
        }
    }
}
