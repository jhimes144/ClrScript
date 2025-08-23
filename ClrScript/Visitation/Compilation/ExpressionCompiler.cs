using ClrScript.Elements.Expressions;
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

        public void VisitAssign(Assign expr)
        {
            if (expr.AssignTo is MemberRootAccess rootAccess)
            {
                expr.Expression.Accept(this);

                if (rootAccess.AccessType == RootMemberAccessType.Variable)
                {
                    _context.CurrentEnv.VariableEmitStoreFromEvalStack(rootAccess.Name.Value);
                }
                else
                {
                    // external type
                }
            }
            else if (expr.AssignTo is MemberAccess memberAccess)
            {
                var generator = _context.CurrentEnv.Generator;

                memberAccess.Expr.Accept(this);
                generator.Emit(OpCodes.Ldstr, memberAccess.Name.Value);
                expr.Expression.Accept(this);

                if (memberAccess.Expr.InferredType == typeof(ClrScriptObject))
                {
                    generator.Emit(OpCodes.Callvirt, typeof(ClrScriptObject)
                            .GetMethod(nameof(ClrScriptObject.Set)));
                }
                else
                {
                    generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.Assign)), null);
                }
            }
        }

        public void VisitBinary(Binary expr)
        {
            var gen = _context.CurrentEnv.Generator;
            expr.Left.Accept(this);
            expr.Right.Accept(this);

            switch (expr.Op.Type)
            {
                case Lexer.TokenType.Plus:
                    if (expr.InferredType == typeof(double))
                    {
                        gen.Emit(OpCodes.Add_Ovf);
                    }
                    else if (expr.InferredType == typeof(string))
                    {
                        gen.EmitCall(OpCodes.Call, typeof(string)
                            .GetMethod(nameof(string.Concat)), null);
                    }
                    else
                    {
                        gen.EmitCall(OpCodes.Call, typeof(Operators)
                            .GetMethod(nameof(Operators.Add)), null);
                    }
                    break;
                case Lexer.TokenType.Minus:
                    gen.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.Subtract)), null);
                    break;
                case Lexer.TokenType.Multiply:
                    gen.EmitCall(OpCodes.Call, typeof(Operators)
                            .GetMethod(nameof(Operators.Multiply)), null);
                    break;
                case Lexer.TokenType.Divide:
                    gen.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.Divide)), null);
                    break;
                case Lexer.TokenType.EqualEqual:
                    gen.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.EqualEqual)), null);
                    break;
                case Lexer.TokenType.BangEqual:
                    gen.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.EqualEqual)), null);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ceq);
                    break;
                case Lexer.TokenType.GreaterThan:
                    gen.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.GreaterThan)), null);
                    break;
                case Lexer.TokenType.LessThan:
                    gen.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.LessThan)), null);
                    break;
                case Lexer.TokenType.GreaterThanOrEqual:
                    if (expr.InferredType == typeof(double))
                    {
                        _context.CurrentEnv.Generator.Emit(OpCodes.Clt);
                        _context.CurrentEnv.Generator.Emit(OpCodes.Ldc_I4_0);
                        _context.CurrentEnv.Generator.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        gen.EmitCall(OpCodes.Call, typeof(Operators)
                            .GetMethod(nameof(Operators.GreaterThanOrEqual)), null);
                    }
                    break;
                case Lexer.TokenType.LessThanOrEqual:
                    if (expr.InferredType == typeof(double))
                    {
                        _context.CurrentEnv.Generator.Emit(OpCodes.Cgt);
                        _context.CurrentEnv.Generator.Emit(OpCodes.Ldc_I4_0);
                        _context.CurrentEnv.Generator.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        gen.EmitCall(OpCodes.Call, typeof(Operators)
                            .GetMethod(nameof(Operators.LessThanOrEqual)), null);
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

        public void VisitMemberAccess(MemberAccess memberAccess)
        {
            memberAccess.Expr.Accept(this);
            var generator = _context.CurrentEnv.Generator;

            if (memberAccess.Expr.InferredType == typeof(ClrScriptObject))
            {
                generator.Emit(OpCodes.Ldstr, memberAccess.Name.Value);
                generator.Emit(OpCodes.Callvirt, typeof(ClrScriptObject)
                    .GetMethod(nameof(ClrScriptObject.Get)));
            }
            else
            {
                throw new NotImplementedException("dynamic member access");
            }
        }


        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
            var generator = _context.CurrentEnv.Generator;
            generator.Emit(OpCodes.Newobj, typeof(ClrScriptObject)
                .GetConstructor(Type.EmptyTypes));

            foreach (var (key, value) in objLiteral.Properties)
            {
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldstr, key.Value);

                value.Accept(this);
                generator.Emit(OpCodes.Callvirt, typeof(ClrScriptObject)
                            .GetMethod(nameof(ClrScriptObject.Set)));
            }
        }

        public void VisitUnary(Unary expr)
        {
            expr.Right.Accept(this);

            switch (expr.Op.Type)
            {
                case Lexer.TokenType.Minus:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.UnaryMinus)), null);
                    break;

                case Lexer.TokenType.Bang:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.UnaryBang)), null);
                    break;

                case Lexer.TokenType.Plus:
                    // Unary plus - no operation needed, value is already on stack
                    // This is essentially a no-op for numeric types
                    break;

                default:
                    throw new NotImplementedException($"Unary operator {expr.Op.Type} is not implemented");
            }
        }

        public void VisitMemberRootAccess(MemberRootAccess var)
        {
            if (var.AccessType == RootMemberAccessType.Variable)
            {
                _context.CurrentEnv.VariableEmitLoadIntoEvalStack(var.Name.Value);
            }
            
        }

        public void VisitCall(Call call)
        {
            throw new NotImplementedException();
        }
    }
}
