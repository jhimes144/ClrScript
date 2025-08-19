using Clank.Runtime.Builtins;
using Clank.Elements.Statements;
using Clank.Visitation.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Clank.Elements.Expressions;

namespace Clank.Visitation.SemanticAnalysis
{
    class SemanticStatementAnalyzer : IStatementVisitor
    {
        readonly AnalysisContext _context;

        public SemanticStatementAnalyzer(AnalysisContext context)
        {
            _context = context;
        }

        public void VisitBlock(Block block)
        {
            _context.SymbolTable.BeginScope();

            foreach (var statement in block.Statements)
            {
                statement.Accept(this);
            }

            _context.SymbolTable.EndScope();
        }

        public void VisitExprStmt(ExpressionStmt exprStmt)
        {
        }

        public void VisitIfStmt(IfStmt ifStmt)
        {
            ifStmt.Condition.Accept(_context.ExpressionAnalyzer);
            ifStmt.ThenBranch.Accept(this);
            ifStmt.ElseBranch?.Accept(this);
        }

        public void VisitReturnStmt(ReturnStmt returnStmt)
        {
        }

        public void VisitVarStmt(VarStmt varStmt)
        {
            if (varStmt.Initializer is Literal literal && literal.Value == null)
            {
                _context.Errors.Add(new ClankCompileException
                    ("Cannot assign null to implicit typed variable.", varStmt.Name));
                return;
            }

            varStmt.Initializer.Accept(_context.ExpressionAnalyzer);
            _context.SymbolTable.CurrentScope.TryRegisterVariableDeclaration(varStmt);
            _context.SymbolTable.SetType(varStmt, _context.SymbolTable.GetSymbolFor(varStmt.Initializer).Type);
        }

        public void VisitWhileStmt(WhileStmt whileStmt)
        {
        }
    }
}
