using Clank.Elements.Expressions;
using Clank.Elements.Statements;
using Clank.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.SymbolCollection
{
    class SymbolCollectionVisitor : IStatementVisitor, IExpressionVisitor
    {
        readonly SymbolTable _symbolTable;
        readonly List<ClankCompileException> _errors;

        public SymbolCollectionVisitor(SymbolTable symbolTable, List<ClankCompileException> errors)
        {
            _errors = errors;
            _symbolTable = symbolTable;
            _symbolTable.BeginScope(ScopeKind.Root);
        }

        public void VisitAssign(Assign expr)
        {
            if (expr.AssignTo is MemberRootAccess rootAccess)
            {
                expr.Expression.Accept(this);
                expr.InferredType = expr.InferredType;
            }
            else if (expr.AssignTo is MemberAccess memberAccess)
            {
                memberAccess.Expr.Accept(this);
                expr.Expression.Accept(this);
            }
        }

        public void VisitBinary(Binary expr)
        {
            expr.Left.Accept(this);
            expr.Right.Accept(this);

            if (expr.Left.InferredType == expr.Right.InferredType)
            {
                expr.InferredType = expr.Left.InferredType;
            }
        }

        public void VisitBlock(Block block)
        {
            _symbolTable.BeginScope(ScopeKind.Block);

            foreach (var stmt in block.Statements)
            {
                stmt.Accept(this);
            }

            _symbolTable.EndScope();
        }

        public void VisitBlockExpr(BlockExpr blockExpr)
        {
            _symbolTable.BeginScope(ScopeKind.Lambda);

            foreach (var stmt in blockExpr.Block.Statements)
            {
                stmt.Accept(this);
            }

            _symbolTable.EndScope();
        }

        public void VisitCall(Call call)
        {

        }

        public void VisitExprStmt(ExpressionStmt exprStmt)
        {
            exprStmt.Expression.Accept(this);
        }

        public void VisitForStmt(ForStmt forStmt)
        {
            forStmt.Initializer?.Accept(this);
            forStmt.Condition?.Accept(this);
            forStmt.Body.Accept(this);
            forStmt.Increment?.Accept(this);
        }

        public void VisitGrouping(Grouping expr)
        {
            expr.Expression.Accept(this);
        }

        public void VisitIfStmt(IfStmt ifStmt)
        {
            ifStmt.Condition.Accept(this);
            ifStmt.ThenBranch.Accept(this);
            ifStmt.ElseBranch?.Accept(this);
        }

        public void VisitLambda(Lambda lambda)
        {
            lambda.Body.Accept(this);
        }

        public void VisitLiteral(Literal expr)
        {
            if (expr.Value == null)
            {
                return;
            }

            expr.InferredType = expr.Value.GetType();
        }

        public void VisitLogical(Logical logical)
        {
            logical.Left.Accept(this);
            logical.Right.Accept(this);

            if (logical.Left.InferredType == logical.Right.InferredType)
            {
                logical.InferredType = logical.Left.InferredType;
            }
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
            objLiteral.InferredType = typeof(ClankObject);
        }

        public void VisitMemberAccess(MemberAccess memberAccess)
        {
            memberAccess.Expr.Accept(this);
        }

        public void VisitReturnStmt(ReturnStmt returnStmt)
        {
            returnStmt.Expression.Accept(this);
        }

        public void VisitUnary(Unary expr)
        {
            expr.Right.Accept(this);
            expr.InferredType = expr.Right.InferredType;
        }

        public void VisitMemberRootAccess(MemberRootAccess member)
        {
            var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                (member.Name.Value, out var foundScopeExist);

            if (existingSymbol != null)
            {
                if (existingSymbol is VariableSymbol sym)
                {
                    var stmt = (VarStmt)sym.Element;
                    member.InferredType = stmt.InferredType;
                    member.AccessType = RootMemberAccessType.Variable;
                    return;
                }

                _errors.Add(new ClankCompileException($"'{member.Name.Value}' must point to either a const, eternal, or var declaration.", member));
                return;
            }

            _errors.Add(new ClankCompileException($"Variable '{member.Name.Value}' does not exist.", member));
        }

        public void VisitVarStmt(VarStmt varStmt)
        {
            var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                (varStmt.Name.Value, out var foundScopeExist);

            if (existingSymbol != null)
            {
                if (foundScopeExist == _symbolTable.CurrentScope)
                {
                    _errors.Add(new ClankCompileException($"Variable has a bad name. " +
                        $"'{varStmt.Name.Value}' has already been declared in the current scope.", varStmt));
                }
                else
                {
                    _errors.Add(new ClankCompileException($"Variable has a bad name. " +
                        $"'{varStmt.Name.Value}' has already been declared in an enclosing scope.", varStmt));
                }

                return;
            }

            var symbol = new VariableSymbol(varStmt.Name.Value,
                varStmt, _symbolTable.CurrentScope)
            {
                VariableType = varStmt.VariableType
            };

            _symbolTable.SetSymbolFor(varStmt, symbol);

            varStmt.Initializer.Accept(this);
            varStmt.InferredType = varStmt.Initializer.InferredType;
        }

        public void VisitWhileStmt(WhileStmt whileStmt)
        {
            whileStmt.Condition.Accept(this);
            whileStmt.Body.Accept(this);
        }

        public void VisitPrintStmt(PrintStmt printStmt)
        {
            printStmt.Expression.Accept(this);
        }
    }
}
