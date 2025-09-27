using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClrScript.TypeManagement;
using ClrScript.Runtime;

namespace ClrScript.Visitation.Compilation
{
    class ClrScriptRoot : ClrMethodEnvironment
    {
        public const string IN_FIELD = "<_in>";
        public const string TYPE_MANAGER_FIELD = "<_typeManager>";
        public const string DYN_OPS_FIELD = "<_dynOpsField>";

        public override FieldInfo InField { get; protected set; }
        public override FieldInfo TypeManagerField { get; protected set; }

        public ClrScriptRoot(CompilationContext context) : base(context)
        {
            InField = context.ClrScriptEntryTypeBuilder
                .DefineField(IN_FIELD, context.InType, FieldAttributes.Private);

            TypeManagerField = context.ClrScriptEntryTypeBuilder
                .DefineField(TYPE_MANAGER_FIELD, typeof(TypeManager), FieldAttributes.Private | FieldAttributes.InitOnly);

            var main = context.ClrScriptEntryTypeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(object), new Type[] { context.InType });

            var constructor = context.ClrScriptEntryTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis,
                new Type[] { typeof(TypeManager) });

            var consGen = constructor.GetILGenerator();

            consGen.Emit(OpCodes.Ldarg_0);
            consGen.Emit(OpCodes.Ldarg_1);
            consGen.Emit(OpCodes.Stfld, TypeManagerField);
            consGen.Emit(OpCodes.Ret);

            var mainGen = new ILGeneratorWrapper(main.GetILGenerator());

            mainGen.Emit(OpCodes.Ldarg_0);
            mainGen.Emit(OpCodes.Ldarg_1);
            mainGen.Emit(OpCodes.Stfld, InField);

            Generator = mainGen;
        }
    }
}
