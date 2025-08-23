using ClrScript.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ExternalTypeAnalyzer ExternalTypes { get; }

        public SymbolTable SymbolTable { get; }

        public TypeBuilder RootClrScriptTypeBuilder { get; }

        public ClrScriptRoot Root { get; }

        public ClrMethodEnvironment CurrentEnv { get; private set; }

        public List<ClrScriptModule> Modules { get; }

        public bool ReturnPrepped { get; set; }

        public CompilationContext(ClrScriptCompilationSettings settings,
            SymbolTable symbolTable,
            ExternalTypeAnalyzer externalTypes,
            TypeBuilder rootType)
        {
            RootClrScriptTypeBuilder = rootType;

            SymbolTable = symbolTable;
            ExternalTypes = externalTypes;
            Root = new ClrScriptRoot(this);
            CurrentEnv = Root;
            Modules = new List<ClrScriptModule>();

            StatementCompiler = new StatementCompiler(this);
            ExpressionCompiler = new ExpressionCompiler(this);
        }
    }
}
