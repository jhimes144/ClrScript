using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.SemanticAnalysis
{
    class AnalysisContext
    {
        public SemanticStatementAnalyzer StatementAnalyzer { get; }

        public SemanticExpressionAnalyzer ExpressionAnalyzer { get; }

        public SymbolTable SymbolTable { get; }

        public OperatorResolver OperatorResolver { get; }

        public List<ClankCompileException> Errors { get; }

        public AnalysisContext(ClankCompilationSettings settings)
        {
            Errors = new List<ClankCompileException>();
            SymbolTable = new SymbolTable(this);
            StatementAnalyzer = new SemanticStatementAnalyzer(this);
            ExpressionAnalyzer = new SemanticExpressionAnalyzer(this);
            OperatorResolver = new OperatorResolver(settings);
        }
    }
}
