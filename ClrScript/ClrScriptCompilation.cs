using ClrScript.Interop;
using ClrScript.Lexer;
using ClrScript.Parser;
using ClrScript.Visitation.Compilation;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ClrScriptCompilation<TIn> where TIn : class
    {
        internal Type BuiltRootType { get; }

        internal ClrScriptCompilation(Type builtRootType)
        {
            BuiltRootType = builtRootType;
        }

        public static ClrScriptCompilation<TIn> Compile(ClrScriptIR<TIn> iR)
        {
            if (iR == null)
            {
                throw new ArgumentNullException(nameof(iR));
            }

            if (iR.Errors.Count > 0)
            {
                throw new ClrScriptCompileException("Cannot compile ir, the build contains errors.");
            }

            var assemblyName = new AssemblyName($"ClrScriptDynamic-{Guid.NewGuid()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var clankModule = assemblyBuilder.DefineDynamicModule("ClrScriptModule");
            var defaultClrScript = clankModule.DefineType("Default", TypeAttributes.Public);
            defaultClrScript.AddInterfaceImplementation(typeof(IClrScriptEntry<TIn>));

            var compileContext = new CompilationContext(iR.Settings,
                iR.SymbolTable,
                iR.ExternalTypeAnalyzer,
                defaultClrScript);

            foreach (var statement in iR.Statements)
            {
                statement.Accept(compileContext.StatementCompiler);
            }

            if (!compileContext.ReturnPrepped)
            {
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ldnull);
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ret);
            }

            var clrType = defaultClrScript.CreateType();
            return new ClrScriptCompilation<TIn>(clrType);
        }
    }
}
