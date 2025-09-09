using ClrScript.Elements.Expressions;
using ClrScript.Interop;
using ClrScript.Lexer.TokenReaders;
using ClrScript.Runtime;
using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
                        gen.Emit(OpCodes.Add_Ovf);
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
                        gen.Emit(OpCodes.Sub_Ovf);
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
                        gen.Emit(OpCodes.Mul_Ovf);
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
            throw new NotImplementedException();
        }

        public void VisitGrouping(Grouping expr)
        {
            expr.Expression.Accept(this);
        }

        public void VisitLambda(Lambda lambda)
        {
            throw new NotImplementedException();
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
                _context.CurrentEnv.Generator.Emit(OpCodes.Ldarg_1);
                _context.CurrentEnv.Generator.EmitMemberAccess(_context.ShapeTable.InTypeShape,
                    var.Name.Value, _context.ShapeTable.GetShape(var), _context);
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

        public void VisitCall(Call call)
        {
            var gen = _context.CurrentEnv.Generator;
            var shape = _context.ShapeTable.GetShape(call);

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
                    if (CompileHelpers.GetCanBeOptimized(methodShape.DelegateShape.InferredType, call, _context.ShapeTable))
                    {
                        var methodArgs = methodShape.DelegateShape.InferredType
                            .GetMethod("Invoke")
                            .GetParameters();

                        for (var i = 0; i < methodArgs.Length; i++)
                        {
                            var methodArgType = methodArgs[i].ParameterType;
                            var arg = call.Arguments[i];

                            arg.Accept(this);

                            if (InteropHelpers.GetIsSupportedNumericInteropTypeNeedingConversion(methodArgType))
                            {
                                gen.EmitStackDoubleToNumericValueTypeConversion(methodArgType);
                            }
                        }

                        var invokeMethod = methodShape.InferredType.GetMethod("Invoke");
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
