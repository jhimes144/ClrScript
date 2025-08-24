using ClrScript.Elements.Expressions;
using ClrScript.Elements.Statements;
using ClrScript.Interop;
using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation
{
    class AnalyzerVisitor : IStatementVisitor, IExpressionVisitor
    {
        readonly SymbolTable _symbolTable;
        readonly List<ClrScriptCompileError> _errors;
        readonly ExternalTypeAnalyzer _externalTypeAnalyzer;

        public AnalyzerVisitor(SymbolTable symbolTable,
            ExternalTypeAnalyzer externalTypeAnalyzer,
            List<ClrScriptCompileError> errors)
        {
            _errors = errors;
            _symbolTable = symbolTable;
            _externalTypeAnalyzer = externalTypeAnalyzer;
            _symbolTable.BeginScope(ScopeKind.Root);
        }

        public void VisitAssign(Assign expr)
        {
            if (expr.AssignTo is MemberRootAccess rootAccess)
            {
                expr.Expression.Accept(this);

                var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                    (rootAccess.Name.Value, out _);

                if (existingSymbol != null)
                {
                    // check if our assign does not match the declaration variable inferred type.
                    var declaration = existingSymbol.Element as VarStmt;

                    if (declaration.InferredType != expr.Expression.InferredType)
                    {
                        // we no longer can know (if we ever did) the inferred type of the variable.
                        declaration.InferredType = null;
                    }

                    rootAccess.AccessType = RootMemberAccessType.Variable;
                    rootAccess.InferredType = declaration.InferredType;
                    return;
                }
                else
                {
                    var externalMember = _externalTypeAnalyzer.InType.FindMemberByName(rootAccess.Name.Value);

                    if (externalMember != null)
                    {
                        if (externalMember is ExternalTypeProperty prop)
                        {
                            if (prop.Property.GetSetMethod() == null)
                            {
                                _errors.Add(new ClrScriptCompileError($"'{rootAccess.Name.Value}' is read-only.", rootAccess));
                                return;
                            }

                            rootAccess.AccessType = RootMemberAccessType.External;
                            rootAccess.InferredType = prop.Property.PropertyType;
                            rootAccess.InferredProperty = prop.Property;
                            return;
                        }
                    }
                }
            }
            else if (expr.AssignTo is MemberAccess memberAccess)
            {
                memberAccess.Expr.Accept(this);
                expr.Expression.Accept(this);
                return;
            }

            _errors.Add(new ClrScriptCompileError($"The left hand of an assignment must be variable, property, or indexer.", expr));
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
            objLiteral.InferredType = typeof(ClrScriptObject);
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

                _errors.Add(new ClrScriptCompileError($"'{member.Name.Value}' must point to either a const, eternal, or var declaration.", member));
                return;
            }
            else
            {
                var externalMember = _externalTypeAnalyzer.InType.FindMemberByName(member.Name.Value);

                if (externalMember != null)
                {
                    if (externalMember is ExternalTypeProperty prop)
                    {
                        if (prop.Property.GetGetMethod() == null)
                        {
                            _errors.Add(new ClrScriptCompileError($"'{member.Name.Value}' cannot be read.", member));
                            return;
                        }

                        member.AccessType = RootMemberAccessType.External;
                        member.InferredType = prop.Property.PropertyType;
                        member.InferredProperty = prop.Property;
                        return;
                    }
                }
            }

            _errors.Add(new ClrScriptCompileError($"Variable '{member.Name.Value}' does not exist.", member));
        }

        public void VisitVarStmt(VarStmt varStmt)
        {
            var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                (varStmt.Name.Value, out var foundScopeExist);

            if (existingSymbol != null)
            {
                if (foundScopeExist == _symbolTable.CurrentScope)
                {
                    _errors.Add(new ClrScriptCompileError($"Variable has a bad name. " +
                        $"'{varStmt.Name.Value}' has already been declared in the current scope.", varStmt));
                }
                else
                {
                    _errors.Add(new ClrScriptCompileError($"Variable has a bad name. " +
                        $"'{varStmt.Name.Value}' has already been declared in an enclosing scope.", varStmt));
                }

                return;
            }

            var externalMember = _externalTypeAnalyzer.InType.FindMemberByName(varStmt.Name.Value);

            if (externalMember != null)
            {
                _errors.Add(new ClrScriptCompileError($"Variable has a bad name. " +
                    $"'{varStmt.Name.Value}' is reserved by a system implementation.", varStmt));
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
