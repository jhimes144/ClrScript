using Clank.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.Compilation
{
    class CompilationContext
    {
        public ClankCompilationSettings Settings { get; }

        public StatementCompiler StatementCompiler { get; }

        public ExpressionCompiler ExpressionCompiler { get; }

        public ExternalTypeAnalyzer ExternalTypes { get; }

        public SymbolTable SymbolTable { get; }

        public TypeBuilder RootClankTypeBuilder { get; }

        public ClankRoot Root { get; }

        public ClrMethodEnvironment CurrentEnv { get; private set; }

        public List<ClankModule> Modules { get; }

        public bool ReturnPrepped { get; set; }

        public CompilationContext(ClankCompilationSettings settings,
            SymbolTable symbolTable,
            ExternalTypeAnalyzer externalTypes,
            TypeBuilder rootType)
        {
            RootClankTypeBuilder = rootType;

            SymbolTable = symbolTable;
            ExternalTypes = externalTypes;
            Root = new ClankRoot(this);
            CurrentEnv = Root;
            Modules = new List<ClankModule>();

            StatementCompiler = new StatementCompiler(this);
            ExpressionCompiler = new ExpressionCompiler(this);
        }
    }
}
