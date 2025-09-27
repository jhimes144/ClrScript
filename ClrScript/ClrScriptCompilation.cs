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
using System.Diagnostics;
using ClrScript.TypeManagement;

namespace ClrScript
{
    public class ClrScriptCompilation<TIn> where TIn : class
    {
        internal Type BuiltRootType { get; }
        internal TypeManager TypeManager { get; }
        
        public bool DynamicOperationsEmitted { get; }

        internal ClrScriptCompilation(Type builtRootType, TypeManager typeManager, bool dynamicOperationsEmitted)
        {
            BuiltRootType = builtRootType;
            TypeManager = typeManager;
            DynamicOperationsEmitted = dynamicOperationsEmitted;
        }

        public static ClrScriptCompilation<TIn> Compile(ClrScriptIR<TIn> iR)
        {
            if (iR == null)
            {
                throw new ArgumentNullException(nameof(iR));
            }

            if (iR.Errors.Count > 0)
            {
                throw new ClrScriptCompileException("Cannot compile ir, the build contains errors." + Environment.NewLine 
                    + string.Join(Environment.NewLine, iR.Errors.Select(e => e.Message)));
            }

            var assemblyName = new AssemblyName($"ClrScriptDynamic-{Guid.NewGuid()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var clrScriptModule = assemblyBuilder.DefineDynamicModule("ClrScriptModule");

            var clrScriptEntry = clrScriptModule.DefineType("Entry", TypeAttributes.Public);
            clrScriptEntry.AddInterfaceImplementation(typeof(IClrScriptEntry<TIn>));

            var generator = iR.ShapeTable.CreateTypeGenerator(clrScriptEntry);
            generator.GenerateRuntimeTypes(clrScriptModule);

            var compileContext = new CompilationContext(iR.Settings,
                iR.SymbolTable,
                iR.ShapeTable,
                iR.TypeManager,
                generator,
                clrScriptEntry,
                iR.InType);

            foreach (var statement in iR.Statements)
            {
                statement.Accept(compileContext.StatementCompiler);
            }

            Debug.Assert(compileContext.CurrentEnv.Generator.CompilerStack.Count == 0);

            if (!compileContext.ReturnPrepped)
            {
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ldnull);
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ret);
            }

            var clrType = clrScriptEntry.CreateType();
            return new ClrScriptCompilation<TIn>(clrType, iR.TypeManager, compileContext.DynamicOperationsEmitted);
        }
    }
}
