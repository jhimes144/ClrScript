using Clank.Elements.Expressions;
using Clank.Elements.Statements;
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

        string _currentBlueprintName;

        public SymbolCollectionVisitor(SymbolTable symbolTable, List<ClankCompileException> errors)
        {
            _errors = errors;
            _symbolTable = symbolTable;
            _symbolTable.BeginScope(ScopeKind.Root);
        }

        public void VisitAssign(Assign expr)
        {
            expr.Expression.Accept(this);
        }

        public void VisitBinary(Binary expr)
        {
            expr.Left.Accept(this);
            expr.Right.Accept(this);
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
            throw new NotImplementedException();
        }

        public void VisitLiteral(Literal expr)
        {
            
        }

        public void VisitLogical(Logical logical)
        {
            logical.Left.Accept(this);
            logical.Right.Accept(this);
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
           
        }

        public void VisitMemberAccess(MemberAccess memberAccess)
        {

        }

        public void VisitReturnStmt(ReturnStmt returnStmt)
        {
            returnStmt.Expression.Accept(this);
        }

        public void VisitUnary(Unary expr)
        {
            expr.Right.Accept(this);
        }

        public void VisitMemberRootAccess(MemberRootAccess member)
        {
            var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                (member.Name.Value, out var foundScopeExist);

            if (existingSymbol != null)
            {
                if (existingSymbol is VariableSymbol sym)
                {
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
        }

        public void VisitWhileStmt(WhileStmt whileStmt)
        {
            whileStmt.Condition.Accept(this);
            whileStmt.Body.Accept(this);
        }

        public void VisitBlueprintStmt(BlueprintStmt blueprintStmt)
        {
            if (_symbolTable.CurrentScope.Kind != ScopeKind.Root)
            {
                _errors.Add(new ClankCompileException("Blueprints cannot be declared in inner scopes.", blueprintStmt));
                return;
            }

            _currentBlueprintName = blueprintStmt.Name.Value;

            var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
               (_currentBlueprintName, out _);

            if (existingSymbol != null)
            {
                _errors.Add(new ClankCompileException($"Blueprint has a bad name. " +
                    $"'{_currentBlueprintName}' has already been declared.", blueprintStmt));

                return;
            }

            var symbol = new BlueprintSymbol(blueprintStmt.Name.Value, blueprintStmt, _symbolTable.CurrentScope);
            _symbolTable.SetSymbolFor(blueprintStmt, symbol);

            
            _symbolTable.BeginScope(ScopeKind.Blueprint);

            foreach (var stmt in blueprintStmt.Statements)
            {
                stmt.Accept(this);
            }

            _symbolTable.EndScope();
        }

        public void VisitBlueprintPropStmt(BlueprintPropertyStmt blueprintPropStmt)
        {
            var propName = blueprintPropStmt.Name.Value;
            var existingSymbol = _symbolTable.CurrentScope.FindLocalSymbol(propName);

            if (existingSymbol != null)
            {
                _errors.Add(new ClankCompileException($"'{propName}' has already been declared in blueprint '{_currentBlueprintName}'.",
                    blueprintPropStmt));
                return;
            }

            var symbol = new BlueprintPropSymbol(_currentBlueprintName, propName, blueprintPropStmt.Type.Value,
                blueprintPropStmt, _symbolTable.CurrentScope);

            _symbolTable.SetSymbolFor(blueprintPropStmt, symbol);
        }
    }
}
