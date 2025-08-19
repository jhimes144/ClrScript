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

        public ClankRoot Root { get; }

        public ClrMethodEnvironment CurrentEnv { get; private set; }

        public List<ClankModule> Modules { get; }

        public bool ReturnPrepped { get; set; }

        public CompilationContext(ClankCompilationSettings settings,
            TypeBuilder clankType, Type inType, Type outType)
        {
            Root = new ClankRoot(clankType, inType, outType);
            CurrentEnv = Root;
            Modules = new List<ClankModule>();

            StatementCompiler = new StatementCompiler(this);
            ExpressionCompiler = new ExpressionCompiler(this);
        }
    }
}
