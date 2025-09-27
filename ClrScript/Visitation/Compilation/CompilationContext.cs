using ClrScript.Interop;
using ClrScript.TypeManagement;
using ClrScript.Visitation.Analysis;
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
        readonly Stack<ClrMethodEnvironment> _envs = new Stack<ClrMethodEnvironment>();

        public ClrScriptCompilationSettings Settings { get; }

        public StatementCompiler StatementCompiler { get; }

        public ExpressionCompiler ExpressionCompiler { get; }

        public TypeManager TypeManager { get; }

        public TypeGenerator TypeGenerator { get; }

        public SymbolTable SymbolTable { get; }

        public ShapeTable ShapeTable { get; }

        public TypeBuilder ClrScriptEntryTypeBuilder { get; }

        public ClrScriptRoot Root { get; }

        public ClrMethodEnvironment CurrentEnv => _envs.Peek();

        public bool ReturnPrepped { get; set; }

        public MethodInfo PrintStmtMethod { get; }

        public bool DynamicOperationsEmitted { get; set; }

        public Type InType { get; }

        public CompilationContext(ClrScriptCompilationSettings settings,
            SymbolTable symbolTable,
            ShapeTable shapeTable,
            TypeManager typeManager,
            TypeGenerator typeGenerator,
            TypeBuilder rootType,
            Type inType)
        {
            typeManager.ValidatePrepareType(inType);

            if (typeof(IImplementsPrintStmt).IsAssignableFrom(inType))
            {
                PrintStmtMethod = typeof(IImplementsPrintStmt).GetMethod
                    (nameof(IImplementsPrintStmt.Print), new Type[] { typeof(object) });
            }

            InType = inType;
            ClrScriptEntryTypeBuilder = rootType;

            ShapeTable = shapeTable;
            SymbolTable = symbolTable;
            TypeManager = typeManager;
            TypeGenerator = typeGenerator;
            Root = new ClrScriptRoot(this);
            _envs.Push(Root);

            SymbolTable.BeginRootScope();
            StatementCompiler = new StatementCompiler(this);
            ExpressionCompiler = new ExpressionCompiler(this);
        }

        public void EnterEnvironment(ClrMethodEnvironment env)
        {
            _envs.Push(env);
        }

        public void ExitEnvironment()
        {
            _envs.Pop();
        }
    }
}
