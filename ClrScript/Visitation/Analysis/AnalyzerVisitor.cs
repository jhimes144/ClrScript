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

namespace ClrScript.Visitation.Analysis
{
    class AnalyzerVisitor : IStatementVisitor, IExpressionVisitor
    {
        readonly SymbolTable _symbolTable;
        readonly List<ClrScriptCompileError> _errors;
        readonly TypeManager _typeManager;
        readonly ShapeTable _shapeTable;
        readonly Type _inType;

        readonly LambdaAnalysisRecursionDetector _recursionDetector 
            = new LambdaAnalysisRecursionDetector();

        readonly List<Lambda> _visitedLambdas = new List<Lambda>();
        readonly Stack<LambdaTracking> _lambdaTrackingStack = new Stack<LambdaTracking>();

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
            _symbolTable.DeclareRootScope();
        }

        public void ShapeUnShapedLambdas()
        {
            foreach (var lambda in _visitedLambdas)
            {
                var methodShape = (MethodShape)_shapeTable.GetShape(lambda);

                if (methodShape.CallSignature == null || methodShape.IsStowaway)
                {
                    methodShape.CallSignature = null;
                    insureLambdaCallSignature(methodShape, lambda.Parameters.Select(_ => UnknownShape.Instance).ToArray());
                }
            }
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
                    rootAccess.AccessType = RootMemberAccessType.Variable;

                    var declaration = existingSymbol.Element as VarStmt;
                    var derivedShape = _shapeTable.DeriveShape(declaration, assignStmt.ExprAssignValue);
                    _shapeTable.SetShape(declaration, derivedShape, true);

                    
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
                    throw new NotImplementedException();
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
            _symbolTable.DeclareScope(block, ScopeKind.Block);

            foreach (var stmt in block.Statements)
            {
                stmt.Accept(this);
            }

            _symbolTable.EndScope();
        }

        public void VisitBlockExpr(BlockExpr blockExpr)
        {
            blockExpr.Block.Accept(this);
        }

        public void VisitCall(Call call)
        {
            call.Callee.Accept(this);
            var calleeShape = _shapeTable.GetShape(call.Callee);

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

            if (calleeShape is MethodShape methodShape)
            {
                var argShapes = new List<ShapeInfo>();

                foreach (var arg in call.Arguments)
                {
                    argShapes.Add(_shapeTable.GetShape(arg));
                }

                if (!methodShape.IsTypeMethod)
                {
                    if (!insureLambdaCallSignature(methodShape, argShapes))
                    {
                        // we are in recursion or arg count mismatch. override callee to unknown shape
                        //_shapeTable.SetShape(call.Callee, UnknownShape.Instance, true);
                        //_shapeTable.SetShape(call, calleeShape);
                        return;
                    }
                }

                _shapeTable.SetShape(call, new MethodReturnShape(methodShape));
            }
            else
            {
                _shapeTable.SetShape(call, UnknownShape.Instance);
            }
        }

        bool insureLambdaCallSignature(MethodShape methodShape, IReadOnlyList<ShapeInfo> argShapes)
        {
            if (argShapes.Count != methodShape.Declaration.Parameters.Count)
            {
                return false;
            }

            var toUseArgShapes = argShapes.ToArray();

            if (methodShape.CallSignature != null)
            {
                if (methodShape.CallSignature
                    .Arguments.SequenceEqual(argShapes))
                {
                    return true;
                }

                for (var i = 0; i < argShapes.Count; i++)
                {
                    var callArgShape = argShapes[i];
                    var sigArgShape = methodShape.CallSignature.Arguments[i];

                    var derived = _shapeTable.DeriveShape(sigArgShape, callArgShape);
                    toUseArgShapes[i] = derived;
                }
            }

            _symbolTable.BeginScope(methodShape.Declaration);
            _symbolTable.CurrentScope.ClearSymbols();
            _symbolTable.DestroyChildren(_symbolTable.CurrentScope);

            var index = 0;

            foreach (var param in methodShape.Declaration.Parameters)
            {
                var existingSymbol = _symbolTable.CurrentScope.FindSymbolGoingUp
                    (param.Value, out var foundScopeExist);

                if (existingSymbol != null)
                {
                    _errors.Add(new ClrScriptCompileError($"Function parameter has a bad name. " +
                            $"'{param.Value}' has already been declared in an enclosing scope.", methodShape.Declaration));

                    return false;
                }

                var symbol = new LambdaParamSymbol(index, param.Value,
                    methodShape.Declaration, _symbolTable.CurrentScope);

                index++;
            }

            // check if we are entering recursion
            if (_recursionDetector.Enter(methodShape.Declaration))
            {
                return false;
            }

            var lambdaTrack = new LambdaTracking(toUseArgShapes);
            _lambdaTrackingStack.Push(lambdaTrack);
            _shapeTable.EnterShapeScope();

            methodShape.Declaration.Body.Accept(this);

            var shapes = _shapeTable.EndShapeScope();
            _symbolTable.EndScope();

            _lambdaTrackingStack.Pop();

            Debug.Assert(!(lambdaTrack.ReturnShape is UndeterminedShape));

            methodShape.CallSignature = new MethodCallSignature(shapes, lambdaTrack.ReturnShape,
                toUseArgShapes, lambdaTrack.CapturedVariables);

            _recursionDetector.RollbackTo(methodShape.Declaration);

            return true;
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
            _visitedLambdas.Add(lambda);
            var methodShape = new MethodShape(lambda);
            _shapeTable.SetShape(lambda, methodShape);

            // create an empty scope for now, will be used when we visit the lambda call
            _symbolTable.DeclareScope(lambda, ScopeKind.Lambda);
            _symbolTable.EndScope();
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
                if (existingSymbol is VariableSymbol varSym)
                {
                    var stmt = (VarStmt)varSym.Element;
                    _shapeTable.SetShape(member, _shapeTable.GetShape(stmt));
                    member.AccessType = RootMemberAccessType.Variable;
                    return;
                }
                else if (existingSymbol is LambdaParamSymbol paramSym)
                {
                    var lambda = (Lambda)paramSym.Element;
                    var paramShape = _lambdaTrackingStack.Peek().ArgShapes[paramSym.ParamIndex];
                    member.AccessType = RootMemberAccessType.LambdaArg;
                    member.ParamIndex = paramSym.ParamIndex;

                    _shapeTable.SetShape(member, paramShape);
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

            if (_lambdaTrackingStack.Count > 0)
            {
                var returnShape = _shapeTable.GetShape(returnStmt.Expression);
                var lambdaTrack = _lambdaTrackingStack.Peek();

                lambdaTrack.ReturnShape = _shapeTable.DeriveShape(lambdaTrack.ReturnShape, returnShape);
            }
        }

        public void VisitUnary(Unary expr)
        {
            expr.Right.Accept(this);
            var innerShape = _shapeTable.GetShape(expr.Right);
            _shapeTable.SetShape(expr, innerShape);
        }

        public void VisitVarStmt(VarStmt varStmt)
        {
            if (varStmt.Initializer != null)
            {
                varStmt.Initializer.Accept(this);
                _shapeTable.SetShape(varStmt, _shapeTable.GetShape(varStmt.Initializer));
            }

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
                    .Cast<ShapeInfo>()
                    .ToList();

                return new MethodShape(returnShape, args);
            }

            return UnknownShape.Instance;
        }

        class LambdaTracking
        {
            public IReadOnlyList<ShapeInfo> ArgShapes { get; }

            public ShapeInfo ReturnShape { get; set; } = UndeterminedShape.Instance;

            public List<VarStmt> CapturedVariables { get; } = new List<VarStmt>();

            public LambdaTracking(IReadOnlyList<ShapeInfo> argShapes)
            {
                ArgShapes = argShapes;
            }
        }
    }
}
