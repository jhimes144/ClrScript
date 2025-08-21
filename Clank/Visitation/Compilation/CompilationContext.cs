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

        public TypeBuilder RootClankTypeBuilder { get; }

        public Type InType { get; }

        public Type OutType { get; }

        public ClankRoot Root { get; }

        public ClrMethodEnvironment CurrentEnv { get; private set; }

        public List<ClankModule> Modules { get; }

        public bool ReturnPrepped { get; set; }

        public CompilationContext(ClankCompilationSettings settings,
            ExternalTypeAnalyzer externalTypes,
            TypeBuilder rootType, Type inType, Type outType)
        {
            InType = inType;
            OutType = outType;
            RootClankTypeBuilder = rootType;

            Root = new ClankRoot(this);
            CurrentEnv = Root;
            ExternalTypes = externalTypes;
            Modules = new List<ClankModule>();

            StatementCompiler = new StatementCompiler(this);
            ExpressionCompiler = new ExpressionCompiler(this);
        }
    }
}
