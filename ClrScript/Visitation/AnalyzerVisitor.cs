using ClrScript.Elements.Expressions;
using ClrScript.Elements.Statements;
using ClrScript.Interop;
using ClrScript.Runtime.Builtins;
using ClrScript.TypeManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation
{
    class AnalyzerVisitor : IStatementVisitor, IExpressionVisitor
    {
        readonly SymbolTable _symbolTable;
        readonly List<ClrScriptCompileError> _errors;
        readonly TypeManager _typeManager;
        readonly ShapeTable _shapeTable;
        readonly Type _inType;

        public AnalyzerVisitor(SymbolTable symbolTable,
            TypeManager typeManager,
            ShapeTable shapeTable,
            Type inType,
            List<ClrScriptCompileError> errors)
        {
            _errors = errors;
            _inType = inType;
            _shapeTable = shapeTable;
            _typeManager = typeManager;
            _symbolTable = symbolTable;
            _symbolTable.BeginScope(ScopeKind.Root);
        }

        public void VisitAssignStmt(AssignStmt assignStmt)
        {
            if (assignStmt.AssignTo is MemberRootAccess rootAccess)
            {
                assignStmt.ExprAssignValue.Accept(this);

                var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                    (rootAccess.Name.Value, out _);

                if (existingSymbol != null)
                {
                    var declaration = existingSymbol.Element as VarStmt;
                    var derivedShape = _shapeTable.DeriveShape(declaration, assignStmt.ExprAssignValue);
                    _shapeTable.SetShape(declaration, derivedShape, true);

                    rootAccess.AccessType = RootMemberAccessType.Variable;
                    _shapeTable.SetShape(rootAccess, derivedShape);
                    return;
                }
                else
                {
                    var externalMember = _typeManager.GetTypeInfo(_inType)
                        .GetMember(rootAccess.Name.Value);

                    if (externalMember != null)
                    {
                        if (externalMember is PropertyInfo prop)
                        {
                            if (prop.GetSetMethod() == null)
                            {
                                _errors.Add(new ClrScriptCompileError($"'{rootAccess.Name.Value}' is read-only.", rootAccess));
                                return;
                            }

                            rootAccess.AccessType = RootMemberAccessType.External;
                            _shapeTable.SetShape(rootAccess, new TypeShape(prop.PropertyType));
                            return;
                        }

                        if (externalMember is FieldInfo field)
                        {
                            rootAccess.AccessType = RootMemberAccessType.External;
                            _shapeTable.SetShape(rootAccess, new TypeShape(field.FieldType));
                            return;
                        }
                    }
                }
            }
            else if (assignStmt.AssignTo is MemberAccess assignToMemberAccess)
            {
                assignToMemberAccess.Expr.Accept(this);
                assignStmt.ExprAssignValue.Accept(this);

                var expressionShape = _shapeTable.GetShape(assignStmt.ExprAssignValue);
                var assigneeShape = _shapeTable.GetShape(assignToMemberAccess.Expr);

                if (assigneeShape is ClrScriptObjectShape clrObjectAssigneeShape)
                {
                    var propName = assignToMemberAccess.Name.Value;
                    var propShape = clrObjectAssigneeShape.ShapeInfoByPropName
                        .GetValueOrDefault(propName);

                    if (propShape != null)
                    {
                        var derivedPropShape = _shapeTable.DeriveShape(propShape, expressionShape);
                        clrObjectAssigneeShape.SetShapeInfoForProp(propName, derivedPropShape);
                    }
                    else
                    {
                        // property has never been assigned.
                        clrObjectAssigneeShape.SetShapeInfoForProp(propName, expressionShape);
                    }
                }

                return;
            }
            else if (assignStmt.AssignTo is Indexer indexer)
            {
                indexer.Accept(this);
                assignStmt.ExprAssignValue.Accept(this);

                var expressionShape = _shapeTable.GetShape(assignStmt.ExprAssignValue);
                var indexerShape = _shapeTable.GetShape(indexer);

                var newIndexerShape = _shapeTable.DeriveShape(indexerShape, expressionShape);

                var calleeShape = _shapeTable.GetShape(indexer.Callee);

                if (calleeShape is ClrScriptArrayShape arrayShape)
                {
                    arrayShape.ContentShape = _shapeTable.DeriveShape
                        (arrayShape.ContentShape, newIndexerShape);

                    return;
                }
                else if (calleeShape is TypeShape typeShape)
                {
                    return;
                    //var indexerProp = _typeManager
                    //    .GetTypeInfo(typeShape.InferredType)?
                    //    .GetIndexer();

                    //if (indexerProp != null)
                    //{
                    //    _shapeTable.SetShape(indexer, new TypeShape(indexerProp.PropertyType));
                    //    return;
                    //}
                }
            }

            _errors.Add(new ClrScriptCompileError($"The left hand of an assignment must be variable, property, or indexer.", assignStmt));
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
            call.Callee.Accept(this);

            foreach (var arg in call.Arguments)
            {
                arg.Accept(this);
            }

            // special case: Array adds
            if (call.Callee is MemberAccess memberAccess 
                && memberAccess.Name.Value == "add" 
                && _shapeTable.GetShape(memberAccess.Expr) is ClrScriptArrayShape arrayShape)
            {
                // derive shape and set it as array type
                arrayShape.ContentShape = _shapeTable.DeriveShape(arrayShape.ContentShape,
                    _shapeTable.GetShape(call.Arguments[0]));
            }

            _shapeTable.SetShape(call, _shapeTable.GetShape(call.Callee));
        }

        public void VisitIndexer(Indexer indexer)
        {
            indexer.Callee.Accept(this);
            indexer.Expression.Accept(this);

            var calleeShape = _shapeTable.GetShape(indexer.Callee);

            if (calleeShape is ClrScriptArrayShape arrayShape)
            {
                _shapeTable.SetShape(indexer, arrayShape.ContentShape);
                return;
            }
            else if (calleeShape is TypeShape typeShape)
            {
                var indexerProp = _typeManager
                    .GetTypeInfo(typeShape.InferredType)?
                    .GetIndexer();

                if (indexerProp != null)
                {
                    _shapeTable.SetShape(indexer, new TypeShape(indexerProp.PropertyType));
                    return;
                }
            }

            _shapeTable.SetShape(indexer, new UnknownShape());
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
                _shapeTable.SetShape(expr, new TypeShape(typeof(DynamicNull)));
            }
            else
            {
                _shapeTable.SetShape(expr, new TypeShape(expr.Value.GetType()));
            }
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

            _shapeTable.SetShape(objLiteral, new ClrScriptObjectShape(shapeInfoByPropName));
        }

        public void VisitArrayLiteral(ArrayLiteral expr)
        {
            ShapeInfo contentsShape = UndeterminedShape.Instance;

            foreach (var contentExpr in expr.Contents)
            {
                contentExpr.Accept(this);
                var exprShape = _shapeTable.GetShape(contentExpr);

                if (contentsShape is UndeterminedShape)
                {
                    contentsShape = exprShape;
                }
                else
                {
                    contentsShape = _shapeTable.DeriveShape(contentsShape, exprShape);
                }
            }

            _shapeTable.SetShape(expr, new ClrScriptArrayShape(contentsShape));
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
                var globalMember = _typeManager.GetTypeInfo(_inType)?
                        .GetMember(member.Name.Value);

                if (globalMember != null)
                {
                    var shape = memberToShape(globalMember);
                    _shapeTable.SetShape(member, shape);
                    member.AccessType = RootMemberAccessType.External;
                    return;
                }
            }

            _errors.Add(new ClrScriptCompileError($"Variable or built-in '{member.Name.Value}' does not exist.", member));
        }

        public void VisitMemberAccess(MemberAccess memberAccess)
        {
            memberAccess.Expr.Accept(this);
            
            var exprShape = _shapeTable.GetShape(memberAccess.Expr);
            var memberName = memberAccess.Name.Value;

            if (exprShape is ClrScriptObjectShape objShape)
            {
                if (objShape.ShapeInfoByPropName.TryGetValue(memberName, out var propShape))
                {
                    _shapeTable.SetShape(memberAccess, propShape);
                    return;
                }

                var member = _typeManager.GetTypeInfo(typeof(ClrScriptObject))?
                    .GetMember(memberName);

                if (member != null)
                {
                    var shape = memberToShape(member);
                    _shapeTable.SetShape(memberAccess, shape);
                    return;
                }
            }
            else if (exprShape is ClrScriptArrayShape arrayShape)
            {
                var member = _typeManager.GetTypeInfo(typeof(ClrScriptArray<object>))?
                    .GetMember(memberName);

                if (member != null)
                {
                    var shape = memberToShape(member);
                    _shapeTable.SetShape(memberAccess, shape);
                    return;
                }
            }
            else if (exprShape is TypeShape typeShape)
            {
                var member = _typeManager.GetTypeInfo(typeShape.InferredType)?
                    .GetMember(memberName);

                if (member != null)
                {
                    var shape = memberToShape(member);
                    _shapeTable.SetShape(memberAccess, shape);
                    return;
                }
            }

            _shapeTable.SetShape(memberAccess, new UnknownShape());
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

            var externalMember = _typeManager.GetTypeInfo(_inType)?
                        .GetMember(varStmt.Name.Value);

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

            if (varStmt.Initializer != null)
            {
                varStmt.Initializer.Accept(this);
                _shapeTable.SetShape(varStmt, _shapeTable.GetShape(varStmt.Initializer));
            }
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

        public void VisitPostFixUnaryAssignStmt(PostFixUnaryAssignStmt postFixUnaryAssignStmt)
        {
            postFixUnaryAssignStmt.Left.Accept(this);

            if (!(postFixUnaryAssignStmt.Left is MemberRootAccess || postFixUnaryAssignStmt.Left is MemberAccess))
            {
                _errors.Add(new ClrScriptCompileError($"The operand of an increment or decrement operator must be a variable or property.", postFixUnaryAssignStmt));
                return;
            }

            var innerShape = _shapeTable.GetShape(postFixUnaryAssignStmt.Left);
            _shapeTable.SetShape(postFixUnaryAssignStmt, innerShape);
        }

        public void VisitInterpolatedString(InterpolatedStr str)
        {
            throw new NotImplementedException();
        }

        ShapeInfo memberToShape(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo prop)
            {
                if (prop.GetGetMethod() == null)
                {
                    return UnknownShape.Instance;
                }

                return new TypeShape(prop.PropertyType);
            }
            else if (memberInfo is FieldInfo field)
            {
                return new TypeShape(field.FieldType);
            }
            else if (memberInfo is MethodInfo method)
            {
                ShapeInfo returnShape = null;

                if (method.ReturnType != null)
                {
                    var returnType = method.ReturnType == typeof(void)
                        ? typeof(DynamicNull) : method.ReturnType;

                    returnShape = new TypeShape(returnType);
                }

                var args = method.GetParameters()
                    .Select(p => new TypeShape(p.ParameterType))
                    .ToArray();

                return new MethodShape(returnShape, args);
            }

            return UnknownShape.Instance;
        }
    }
}
