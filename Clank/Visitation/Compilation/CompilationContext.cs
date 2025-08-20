using Clank.Interop;
using Clank.Visitation.SemanticAnalysis;
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

        public TypeBuilder RootClankTypeBuilder { get; }

        public Type InType { get; }

        public Type OutType { get; }

        public ClankRoot Root { get; }

        public ClrMethodEnvironment CurrentEnv { get; private set; }

        public AnalysisContext AnalysisContext { get; private set; }

        public List<ClankModule> Modules { get; }

        public bool ReturnPrepped { get; set; }

        public CompilationContext(ClankCompilationSettings settings,
            AnalysisContext analysisContext,
            ExternalTypeAnalyzer externalTypes,
            TypeBuilder rootType, Type inType, Type outType)
        {
            InType = inType;
            OutType = outType;
            RootClankTypeBuilder = rootType;

            Root = new ClankRoot(this);
            CurrentEnv = Root;
            ExternalTypes = externalTypes;
            AnalysisContext = analysisContext;
            Modules = new List<ClankModule>();

            StatementCompiler = new StatementCompiler(this);
            ExpressionCompiler = new ExpressionCompiler(this);
        }
    }
}
