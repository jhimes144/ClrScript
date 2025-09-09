using ClrScript.Interop;
using ClrScript.TypeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Compilation
{
    class CompilationContext
    {
        public ClrScriptCompilationSettings Settings { get; }

        public StatementCompiler StatementCompiler { get; }

        public ExpressionCompiler ExpressionCompiler { get; }

        public TypeManager TypeManager { get; }

        public SymbolTable SymbolTable { get; }

        public ShapeTable ShapeTable { get; }

        public TypeBuilder RootClrScriptTypeBuilder { get; }

        public ClrScriptRoot Root { get; }

        public ClrMethodEnvironment CurrentEnv { get; private set; }

        public List<ClrScriptModule> Modules { get; }

        public bool ReturnPrepped { get; set; }

        public MethodInfo PrintStmtMethod { get; }

        public bool DynamicOperationsEmitted { get; set; }

        public Type InType { get; }

        public CompilationContext(ClrScriptCompilationSettings settings,
            SymbolTable symbolTable,
            ShapeTable shapeTable,
            TypeManager typeManager,
            TypeBuilder rootType,
            Type inType)
        {
            typeManager.ValidateType(inType);

            if (typeof(IImplementsPrintStmt).IsAssignableFrom(inType))
            {
                PrintStmtMethod = typeof(IImplementsPrintStmt).GetMethod
                    (nameof(IImplementsPrintStmt.Print), new Type[] { typeof(object) });
            }

            InType = inType;
            RootClrScriptTypeBuilder = rootType;

            ShapeTable = shapeTable;
            SymbolTable = symbolTable;
            TypeManager = typeManager;
            Root = new ClrScriptRoot(this);
            CurrentEnv = Root;
            Modules = new List<ClrScriptModule>();

            StatementCompiler = new StatementCompiler(this);
            ExpressionCompiler = new ExpressionCompiler(this);
        }
    }
}
