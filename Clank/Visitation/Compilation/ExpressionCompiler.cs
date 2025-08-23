using Clank.Elements.Expressions;
using Clank.Runtime;
using Clank.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.Compilation
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
            expr.Expression.Accept(this);
            _context.CurrentEnv.VariableEmitStoreFromEvalStack(expr.Name.Value);
        }

        public void VisitBinary(Binary expr)
        {
            expr.Left.Accept(this);
            expr.Right.Accept(this);

            switch (expr.Op.Type)
            {
                case Lexer.TokenType.Plus:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.Add)), null);
                    break;
                case Lexer.TokenType.Minus:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.Subtract)), null);
                    break;
                case Lexer.TokenType.Multiply:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                            .GetMethod(nameof(Operators.Multiply)), null);
                    break;
                case Lexer.TokenType.Divide:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.Divide)), null);
                    break;
                case Lexer.TokenType.EqualEqual:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.EqualEqual)), null);
                    // equal equal returns bool unboxed
                    _context.CurrentEnv.Generator.Emit(OpCodes.Box, typeof(bool));
                    break;
                case Lexer.TokenType.BangEqual:
                    // equal equal returns bool unboxed
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.EqualEqual)), null);
                    _context.CurrentEnv.Generator.Emit(OpCodes.Ldc_I4_0);
                    _context.CurrentEnv.Generator.Emit(OpCodes.Ceq);
                    _context.CurrentEnv.Generator.Emit(OpCodes.Box, typeof(bool));
                    break;
                case Lexer.TokenType.GreaterThan:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.GreaterThan)), null);
                    break;
                case Lexer.TokenType.LessThan:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.LessThan)), null);
                    break;
                case Lexer.TokenType.GreaterThanOrEqual:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.GreaterThanOrEqual)), null);
                    break;
                case Lexer.TokenType.LessThanOrEqual:
                    _context.CurrentEnv.Generator.EmitCall(OpCodes.Call, typeof(Operators)
                        .GetMethod(nameof(Operators.LessThanOrEqual)), null);
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
                _context.CurrentEnv.Generator.Emit(OpCodes.Box, typeof(double));
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
                _context.CurrentEnv.Generator.Emit(OpCodes.Box, typeof(bool));
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

            // get the field/property symbol
            //var symbol = _context.AnalysisContext.SymbolTable
            //    .GetSymbolFor(memberAccess);

            //if (symbol.MemberInfo is FieldInfo field)
            //{
            //    _context.CurrentEnv.Generator.Emit(OpCodes.Ldfld, field);
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
            var generator = _context.CurrentEnv.Generator;
            generator.Emit(OpCodes.Newobj, typeof(ClankObject)
                .GetConstructor(Type.EmptyTypes));

            foreach (var (key, value) in objLiteral.Properties)
            {
                generator.Emit(OpCodes.Dup);

                generator.Emit(OpCodes.Ldfld, typeof(ClankObject)
                    .GetField("_properties", BindingFlags.NonPublic | BindingFlags.Instance));

                generator.Emit(OpCodes.Ldstr, key.Value);

                value.Accept(this);
                generator.Emit(OpCodes.Callvirt, typeof(Dictionary<string, object>)
                            .GetMethod("set_Item"));
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
