using ClrScript.Elements.Expressions;
using ClrScript.Interop;
using ClrScript.Lexer.TokenReaders;
using ClrScript.Runtime;
using ClrScript.Runtime.Builtins;
using ClrScript.Visitation.Analysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Compilation
{
    class ExpressionCompiler : IExpressionVisitor
    {
        readonly CompilationContext _context;
        readonly MethodInfo _objEqualsMethod;

        public ExpressionCompiler(CompilationContext context)
        {
            _objEqualsMethod = typeof(object).GetMethod("Equals",
                new[] { typeof(object), typeof(object) });

            _context = context;
        }

        public void VisitBinary(Binary expr)
        {
            var gen = _context.CurrentEnv.Generator;
            var shapeInfo = _context.ShapeTable.GetShape(expr);

            expr.Left.Accept(this);
            gen.EmitBoxIfNeeded(expr, expr.Left, _context.ShapeTable);
            expr.Right.Accept(this);
            gen.EmitBoxIfNeeded(expr, expr.Right, _context.ShapeTable);

            var inferredType = shapeInfo?.InferredType;

            switch (expr.Op.Type)
            {
                case Lexer.TokenType.Plus:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Add);
                    }
                    else if (inferredType == typeof(string))
                    {
                        gen.EmitCall(OpCodes.Call, typeof(string)
                            .GetMethod(nameof(string.Concat)), null);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.Add)), null);
                    }
                    break;
                case Lexer.TokenType.Minus:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Sub);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.Subtract)), null);
                    }
                    break;
                case Lexer.TokenType.Multiply:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Mul);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                                .GetMethod(nameof(DynamicOperations.Multiply)), null);
                    }
                    break;
                case Lexer.TokenType.Divide:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Div);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.Divide)), null);
                    }
                    break;
                case Lexer.TokenType.EqualEqual:
                    if (inferredType == typeof(double) || inferredType == typeof(bool))
                    {
                        gen.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.EqualEqual)), null);
                    }
                    break;
                case Lexer.TokenType.BangEqual:
                    if (inferredType == typeof(double) || inferredType == typeof(bool))
                    {
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.EqualEqual)), null);
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.Emit(OpCodes.Ceq);
                    }
                    break;
                case Lexer.TokenType.GreaterThan:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Cgt);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.GreaterThan)), null);
                    }
                    break;
                case Lexer.TokenType.LessThan:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Clt);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.LessThan)), null);
                    }
                    break;
                case Lexer.TokenType.GreaterThanOrEqual:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Clt);
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.GreaterThanOrEqual)), null);
                    }
                    break;
                case Lexer.TokenType.LessThanOrEqual:
                    if (inferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Cgt);
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.LessThanOrEqual)), null);
                    }
                    break;
                default:
                    throw new Exception("Incorrect token type for op on binary expression.");
            }
        }

        public void VisitBlockExpr(BlockExpr blockExpr)
        {
            blockExpr.Block.Accept(_context.StatementCompiler);
        }

        public void VisitGrouping(Grouping expr)
        {
            expr.Expression.Accept(this);
        }

        public void VisitLambda(Lambda lambda)
        {
            var methodShape = (MethodShape)_context.ShapeTable.GetShape(lambda);
            var sig = methodShape.CallSignature;

            var env = new LambdaMethodEnvironment(sig.GenMethodBuilder, _context);

            _context.SymbolTable.BeginScope(lambda);
            _context.EnterEnvironment(env);
            _context.ShapeTable.EnterShapeScope(sig.ShapesByElement);
            lambda.Body.Accept(this);
            _context.ExitEnvironment();
            _context.ShapeTable.EndShapeScope();
            _context.SymbolTable.EndScope();

            var type = sig.MethodContainerType.CreateType();
            var method = type.GetMethod(sig.GenMethodBuilder.Name);

            //_context.CurrentEnv.Generator.Emit(OpCodes.Ldc_R8, 12d);
            //_context.CurrentEnv.Generator.Emit(OpCodes.Ldc_R8, 12d);
            //_context.CurrentEnv.Generator.Emit(OpCodes.Call, method);
            //_context.CurrentEnv.Generator.Emit(OpCodes.Pop);

            _context.CurrentEnv.Generator.Emit(OpCodes.Ldnull);
            _context.CurrentEnv.Generator.Emit(OpCodes.Ldftn, typeof(TestLambda).GetMethod(nameof(TestLambda.Testf)));

            var delConstructor = sig.GenDelegateType.GetConstructors()[0];
            _context.CurrentEnv.Generator.Emit(OpCodes.Newobj, delConstructor);
        }

        public void VisitLiteral(Literal expr)
        {
            if (expr.Value is double d)
            {
                _context.CurrentEnv.Generator.Emit(OpCodes.Ldc_R8, d);
            }
            else if (expr.Value is string s)
            {
                _context.CurrentEnv.Generator.Emit(OpCodes.Ldstr, s);
            }
            else if (expr.Value is null)
            {
                _context.CurrentEnv.Generator.Emit(OpCodes.Ldnull);
            }
            else if (expr.Value is bool b)
            {
                _context.CurrentEnv.Generator.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            }
            else
            {
                throw new Exception($"Failed to emit literal value {expr.Value}");
            }
        }

        public void VisitLogical(Logical logical)
        {
            var endLabel = _context.CurrentEnv.Generator.DefineLabel();
            var shortCircuitLabel = _context.CurrentEnv.Generator.DefineLabel();
            logical.Left.Accept(this);

            _context.CurrentEnv.Generator.Emit(OpCodes.Dup);

            switch (logical.Op.Type)
            {
                case Lexer.TokenType.DoubleAmpersand:
                    _context.CurrentEnv.Generator.Emit(OpCodes.Brfalse, shortCircuitLabel);
                    _context.CurrentEnv.Generator.Emit(OpCodes.Pop);

                    logical.Right.Accept(this);
                    
                    _context.CurrentEnv.Generator.Emit(OpCodes.Br, endLabel);
                    _context.CurrentEnv.Generator.MarkLabel(shortCircuitLabel);
                    _context.CurrentEnv.Generator.MarkLabel(endLabel);
                    break;

                case Lexer.TokenType.DoublePipe:
                    _context.CurrentEnv.Generator.Emit(OpCodes.Brtrue, shortCircuitLabel);
                    _context.CurrentEnv.Generator.Emit(OpCodes.Pop);

                    logical.Right.Accept(this);

                    _context.CurrentEnv.Generator.Emit(OpCodes.Br, endLabel);
                    _context.CurrentEnv.Generator.MarkLabel(shortCircuitLabel);
                    _context.CurrentEnv.Generator.MarkLabel(endLabel);
                    break;

                default:
                    throw new NotImplementedException($"Logical operator {logical.Op.Type} is not implemented");
            }
        }

        public void VisitMemberRootAccess(MemberRootAccess var)
        {
            if (var.AccessType == RootMemberAccessType.Variable)
            {
                _context.CurrentEnv.VariableEmitLoadIntoEvalStack(var.Name.Value);
            }
            else if (var.AccessType == RootMemberAccessType.External)
            {
                // TODO: Not arg 1 in lambdas
                _context.CurrentEnv.Generator.Emit(OpCodes.Ldarg_1);
                _context.CurrentEnv.Generator.EmitMemberAccess(_context.ShapeTable.InTypeShape,
                    var.Name.Value, _context.ShapeTable.GetShape(var), _context);
            }
            else if (var.AccessType == RootMemberAccessType.LambdaArg)
            {
                // + 1 offset is the in type being passed.
                // TODO: Soon offset will be greater because we also need to pass type manager. Store offset in environment
                _context.CurrentEnv.Generator.EmitLoadArg(var.ParamIndex);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void VisitMemberAccess(MemberAccess memberAccess)
        {
            memberAccess.Expr.Accept(this);
            var generator = _context.CurrentEnv.Generator;

            var memberShapeInfo = _context.ShapeTable.GetShape(memberAccess);
            var objShapeInfo = _context.ShapeTable.GetShape(memberAccess.Expr);

            generator.EmitMemberAccess(objShapeInfo, memberAccess.Name.Value, memberShapeInfo, _context);
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
            var generator = _context.CurrentEnv.Generator;
            var objShapeInfo = _context.ShapeTable.GetShape(objLiteral) as ClrScriptObjectShape 
                ?? throw new Exception("Expecting ClrScriptObjectShape");

            objShapeInfo = objShapeInfo.GetMasterShape();

            generator.Emit(OpCodes.Newobj, objShapeInfo.InferredType
                .GetConstructor(Type.EmptyTypes));

            foreach (var (key, value) in objLiteral.Properties)
            {
                generator.Emit(OpCodes.Dup);
                value.Accept(this);

                generator.EmitBoxIfNeeded(objShapeInfo.ShapeInfoByPropName[key.Value],
                    _context.ShapeTable.GetShape(value));

                generator.Emit(OpCodes.Stfld, objShapeInfo.InferredType.GetField(key.Value));
            }
        }

        public void VisitArrayLiteral(ArrayLiteral expr)
        {
            var gen = _context.CurrentEnv.Generator;
            var arrayShapeInfo = _context.ShapeTable.GetShape(expr) as ClrScriptArrayShape
                ?? throw new Exception("Expecting ClrScriptArrayShape");

            gen.Emit(OpCodes.Newobj, arrayShapeInfo.InferredType
                .GetConstructor(Type.EmptyTypes));

            var arrayType = arrayShapeInfo.InferredType.GenericTypeArguments[0];
            var addMethod = arrayShapeInfo.InferredType.GetMethod
                    ("AddClr", new Type[] { arrayType });

            foreach (var contentExpr in expr.Contents)
            {
                gen.Emit(OpCodes.Dup);
                contentExpr.Accept(this);

                gen.EmitBoxIfNeeded(arrayShapeInfo.ContentShape,
                    _context.ShapeTable.GetShape(contentExpr));

                gen.EmitCall(OpCodes.Callvirt, addMethod, null);
            }
        }

        public void VisitUnary(Unary expr)
        {
            expr.Right.Accept(this);

            _context.CurrentEnv.Generator.EmitBoxIfNeeded
                (expr, expr.Right, _context.ShapeTable);

            var shape = _context.ShapeTable.GetShape(expr);

            switch (expr.Op.Type)
            {
                case Lexer.TokenType.Minus:
                    if (shape.InferredType == typeof(double))
                    {
                        _context.CurrentEnv.Generator.Emit(OpCodes.Neg);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.UnaryMinus)), null);
                    }
                    break;

                case Lexer.TokenType.Bang:
                    if (shape.InferredType == typeof(bool))
                    {
                        _context.CurrentEnv.Generator.Emit(OpCodes.Ldc_I4_0);
                        _context.CurrentEnv.Generator.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.UnaryBang)), null);
                    }
                    break;

                case Lexer.TokenType.Plus:
                    // Unary plus - no operation needed
                    break;

                default:
                    throw new NotImplementedException($"Unary operator {expr.Op.Type} is not implemented");
            }
        }

        public void VisitIndexer(Indexer indexer)
        {
            var gen = _context.CurrentEnv.Generator;
            indexer.Callee.Accept(this);
            indexer.Expression.Accept(this);

            var calleeShape = _context.ShapeTable.GetShape(indexer.Callee);

            if (calleeShape is ClrScriptArrayShape || calleeShape is TypeShape)
            {
                var indexerProp = _context.TypeManager
                    .GetTypeInfo(calleeShape.InferredType)
                    .GetIndexer();

                gen.EmitCall(OpCodes.Callvirt, indexerProp.GetMethod, null);
            }
            else
            {
                _context.DynamicOperationsEmitted = true;
                gen.EmitLoadTypeManager();
                gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.Indexer)), null);
            }
        }

        public void VisitCall(Call call)
        {
            var gen = _context.CurrentEnv.Generator;
            var shape = _context.ShapeTable.GetShape(call.Callee);

            call.Callee.Accept(this);

            if (shape is MethodShape methodShape)
            {
                if (methodShape.IsTypeMethod)
                {
                    var methodInfo = gen.ConsumeCompilerStack<MethodInfo>();
                    
                    if (CompileHelpers.GetCanBeOptimized(methodInfo, call, _context.ShapeTable, out var correctedParams))
                    {
                        var methodArgs = correctedParams;

                        for (var i = 0; i < methodArgs.Length; i++)
                        {
                            var methodArgType = methodArgs[i].ParameterType;
                            var arg = call.Arguments[i];

                            arg.Accept(this);

                            if (InteropHelpers.GetIsSupportedNumericInteropTypeNeedingConversion(methodArgType))
                            {
                                gen.EmitStackDoubleToNumericValueTypeConversion(methodArgType);
                            }

                            gen.EmitBoxIfNeeded(methodArgType, _context.ShapeTable.GetShape(arg).InferredType);
                        }

                        if (methodInfo.IsStatic)
                        {
                            gen.EmitCall(OpCodes.Call, methodInfo, null);
                        }
                        else
                        {
                            gen.EmitCall(OpCodes.Callvirt, methodInfo, null);
                        }

                        if (methodInfo.ReturnType == typeof(void))
                        {
                            gen.Emit(OpCodes.Ldnull);
                        }
                        else if (InteropHelpers.GetIsSupportedNumericInteropTypeNeedingConversion(methodInfo.ReturnType))
                        {
                            gen.Emit(OpCodes.Conv_R8);
                        }
                    }
                    else
                    {
                        var methodName = methodInfo.GetCustomAttribute<ClrScriptMemberAttribute>()
                            .GetMemberName(methodInfo.Name);

                        gen.Emit(OpCodes.Ldstr, methodName);
                        gen.Emit(OpCodes.Ldarg_2); // type manager
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                            .GetMethod(nameof(DynamicOperations.CreateDynCallInfo)), null);
                        gen.EmitDynamicCall(call, this, _context.ShapeTable);
                    }
                }
                else
                {
                    if (CompileHelpers.GetCanBeOptimized(methodShape.CallSignature.GenDelegateType, call, _context.ShapeTable))
                    {
                        var invokeMethod = methodShape.CallSignature.GenDelegateType.GetMethod("Invoke");
                        var methodArgs = invokeMethod.GetParameters();

                        for (var i = 0; i < methodArgs.Length; i++)
                        {
                            var methodArgType = methodArgs[i].ParameterType;
                            var arg = call.Arguments[i];

                            arg.Accept(this);

                            if (InteropHelpers.GetIsSupportedNumericInteropTypeNeedingConversion(methodArgType))
                            {
                                gen.EmitStackDoubleToNumericValueTypeConversion(methodArgType);
                            }

                            gen.EmitBoxIfNeeded(methodArgType, _context.ShapeTable.GetShape(arg).InferredType);
                        }
                        
                        gen.EmitCall(OpCodes.Callvirt, invokeMethod, null);

                        if (invokeMethod.ReturnType == typeof(void))
                        {
                            gen.Emit(OpCodes.Ldnull);
                        }
                        else if (InteropHelpers.GetIsSupportedNumericInteropTypeNeedingConversion(invokeMethod.ReturnType))
                        {
                            gen.Emit(OpCodes.Conv_R8);
                        }
                    }
                    else
                    {
                        _context.DynamicOperationsEmitted = true;
                        gen.EmitDynamicCall(call, this, _context.ShapeTable);
                    }
                }
            }
            else
            {
                _context.DynamicOperationsEmitted = true;
                gen.EmitDynamicCall(call, this, _context.ShapeTable);
            }
        }

        public void VisitInterpolatedString(InterpolatedStr str)
        {
            throw new NotImplementedException();
        }
    }
}
