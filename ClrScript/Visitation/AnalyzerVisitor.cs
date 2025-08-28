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
        readonly ShapeTable _shapeTable;

        public AnalyzerVisitor(SymbolTable symbolTable,
            ExternalTypeAnalyzer externalTypeAnalyzer,
            ShapeTable shapeTable,
            List<ClrScriptCompileError> errors)
        {
            _errors = errors;
            _shapeTable = shapeTable;
            _symbolTable = symbolTable;
            _externalTypeAnalyzer = externalTypeAnalyzer;
            _symbolTable.BeginScope(ScopeKind.Root);
        }

        public void VisitAssign(Assign assignExpr)
        {
            if (assignExpr.AssignTo is MemberRootAccess rootAccess)
            {
                assignExpr.Expression.Accept(this);

                var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                    (rootAccess.Name.Value, out _);

                if (existingSymbol != null)
                {
                    // check if our assign does not match the declaration variable inferred type.
                    var declaration = existingSymbol.Element as VarStmt;
                    var derivedShape = _shapeTable.DeriveShape(declaration, assignExpr.Expression);
                    _shapeTable.SetShape(declaration, derivedShape, true);

                    rootAccess.AccessType = RootMemberAccessType.Variable;
                    _shapeTable.SetShape(rootAccess, derivedShape);
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
                            _shapeTable.SetShape(rootAccess, new BasicShapeInfo(prop.Property.PropertyType));
                            rootAccess.ExternalProperty = prop.Property;
                            return;
                        }
                    }
                }
            }
            else if (assignExpr.AssignTo is MemberAccess assignToMemberAccess)
            {
                assignToMemberAccess.Expr.Accept(this);
                assignExpr.Expression.Accept(this);

                var expressionShape = _shapeTable.GetShape(assignExpr.Expression);
                var assigneeShape = _shapeTable.GetShape(assignToMemberAccess.Expr);

                if (assigneeShape is ClrScriptObjectShapeInfo clrObjectAssigneeShape)
                {
                    var propName = assignToMemberAccess.Name.Value;
                    var propShape = clrObjectAssigneeShape.ShapeInfoByPropName
                        .GetValueOrDefault(propName);

                    var derivedPropShape = _shapeTable.DeriveShape(propShape, expressionShape);
                    clrObjectAssigneeShape.ShapeInfoByPropName[propName] = derivedPropShape;
                }

                return;
            }

            _errors.Add(new ClrScriptCompileError($"The left hand of an assignment must be variable, property, or indexer.", assignExpr));
        }

        public void VisitBinary(Binary expr)
        {
            expr.Left.Accept(this);
            expr.Right.Accept(this);

            var shape = _shapeTable.DeriveShape(expr.Left, expr.Right);
            _shapeTable.SetShape(expr, shape);
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

            _shapeTable.SetShape(expr, new BasicShapeInfo
                (expr.Value.GetType()));
        }

        public void VisitLogical(Logical logical)
        {
            logical.Left.Accept(this);
            logical.Right.Accept(this);

            var derivedShape = _shapeTable.DeriveShape(logical.Left, logical.Right);
            _shapeTable.SetShape(logical, derivedShape);
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
            var shapeInfoByPropName = new Dictionary<string, ShapeInfo>();

            foreach (var (key, value) in objLiteral.Properties)
            {
                value.Accept(this);
                var propShape = _shapeTable.GetShape(value);
                shapeInfoByPropName[key.Value] = propShape;
            }

            _shapeTable.SetShape(objLiteral, new ClrScriptObjectShapeInfo(shapeInfoByPropName));
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
            var innerShape = _shapeTable.GetShape(expr.Right);
            _shapeTable.SetShape(expr, innerShape);
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
                    _shapeTable.SetShape(member, _shapeTable.GetShape(stmt));
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
                        _shapeTable.SetShape(member, new BasicShapeInfo(prop.Property.PropertyType));
                        member.ExternalProperty = prop.Property;
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
            _shapeTable.SetShape(varStmt, _shapeTable.GetShape(varStmt.Initializer));
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
