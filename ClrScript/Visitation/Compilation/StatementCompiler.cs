using ClrScript.Elements.Expressions;
using ClrScript.Elements.Statements;
using ClrScript.Interop;
using ClrScript.Runtime;
using ClrScript.Runtime.Builtins;
using ClrScript.Visitation.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Compilation
{
    class StatementCompiler : IStatementVisitor
    {
        readonly CompilationContext _context;

        public StatementCompiler(CompilationContext context)
        {
            _context = context;
        }

        public void VisitBlock(Block block)
        {
            _context.SymbolTable.BeginScope(block);

            foreach (var stmt in block.Statements)
            {
                stmt.Accept(this);
            }

            _context.SymbolTable.EndScope();
        }

        public void VisitExprStmt(ExpressionStmt exprStmt)
        {
            exprStmt.Expression.Accept(_context.ExpressionCompiler);
            _context.CurrentEnv.Generator.Emit(OpCodes.Pop);
        }

        public void VisitIfStmt(IfStmt ifStmt)
        {
            var generator = _context.CurrentEnv.Generator;
            var elseLabel = generator.DefineLabel();
            var endLabel = generator.DefineLabel();

            ifStmt.Condition.Accept(_context.ExpressionCompiler);
            
            generator.Emit(OpCodes.Brfalse, elseLabel);
            ifStmt.ThenBranch.Accept(this);

            if (generator.InstructionsRendered
                [generator.InstructionsRendered.Count - 1].Code != OpCodes.Ret)
            {
                generator.Emit(OpCodes.Br, endLabel);
            }
            
            generator.MarkLabel(elseLabel);

            ifStmt.ElseBranch?.Accept(this);

            generator.MarkLabel(endLabel);
        }

        public void VisitReturnStmt(ReturnStmt returnStmt)
        {
            returnStmt.Expression.Accept(_context.ExpressionCompiler);

            var expressionShapeInfo = _context.ShapeTable.GetShape(returnStmt.Expression);
            var inferredType = expressionShapeInfo?.InferredType;
            
            if (_context.SymbolTable.CurrentScope.Kind == ScopeKind.Root && (inferredType?.IsValueType ?? false))
            {
                if (InteropHelpers.GetIsSupportedNumericInteropTypeNeedingConversion(inferredType))
                {
                    // emit from before would of already converted the value.
                    inferredType = typeof(double);
                }
                
                _context.CurrentEnv.Generator.Emit(OpCodes.Box, inferredType);
            }
            
            _context.CurrentEnv.Generator.Emit(OpCodes.Ret);
            _context.ReturnPrepped = true;
        }

        public void VisitVarStmt(VarStmt varStmt)
        {
            var varShapeInfo = _context.ShapeTable.GetShape(varStmt);
            var type = varShapeInfo?.InferredType ?? typeof(object);
            _context.CurrentEnv.DeclareVariable(varStmt.Name.Value, type);

            if (varStmt.Initializer != null)
            {
                varStmt.Initializer.Accept(_context.ExpressionCompiler);
                _context.CurrentEnv.Generator.EmitBoxIfNeeded(varStmt, varStmt.Initializer, _context.ShapeTable);

                _context.CurrentEnv.VariableEmitStoreFromEvalStack(varStmt.Name.Value);
            }
        }

        public void VisitWhileStmt(WhileStmt whileStmt)
        {
            var generator = _context.CurrentEnv.Generator;
            var loopStart = generator.DefineLabel();
            var loopEnd = generator.DefineLabel();
            generator.MarkLabel(loopStart);

            whileStmt.Condition.Accept(_context.ExpressionCompiler);
            generator.Emit(OpCodes.Brfalse, loopEnd);
            whileStmt.Body.Accept(this);
            generator.Emit(OpCodes.Br, loopStart);
            generator.MarkLabel(loopEnd);
        }

        public void VisitForStmt(ForStmt forStmt)
        {
            var generator = _context.CurrentEnv.Generator;
            var loopStart = generator.DefineLabel();
            var loopEnd = generator.DefineLabel();
            var incrementStart = generator.DefineLabel();

            // Initialize
            forStmt.Initializer?.Accept(this);

            // Loop condition check
            generator.MarkLabel(loopStart);
            if (forStmt.Condition != null)
            {
                forStmt.Condition.Accept(_context.ExpressionCompiler);
                generator.Emit(OpCodes.Brfalse, loopEnd);
            }

            // Body
            forStmt.Body.Accept(this);

            // Increment
            generator.MarkLabel(incrementStart);
            forStmt.Increment?.Accept(this);

            // Jump back to condition
            generator.Emit(OpCodes.Br, loopStart);
            
            // End of loop
            generator.MarkLabel(loopEnd);
        }

        public void VisitPrintStmt(PrintStmt printStmt)
        {
            var generator = _context.CurrentEnv.Generator;

            if (_context.PrintStmtMethod != null)
            {
                generator.Emit(OpCodes.Ldarg_1);
                printStmt.Expression.Accept(_context.ExpressionCompiler);
                generator.EmitCall(OpCodes.Callvirt, _context.PrintStmtMethod, null);
            }
        }

        public void VisitAssignStmt(AssignStmt assignStmt)
        {
            var gen = _context.CurrentEnv.Generator;

            if (assignStmt.AssignTo is MemberRootAccess rootAccess)
            {
                gen.EmitAssign(rootAccess, () => assignStmt.ExprAssignValue.Accept(_context.ExpressionCompiler),
                    _context.ShapeTable.GetShape(assignStmt.ExprAssignValue), _context);
            }
            else if (assignStmt.AssignTo is MemberAccess assignToMemberAccess)
            {
                gen.EmitAssign(assignToMemberAccess, () => assignStmt.ExprAssignValue.Accept(_context.ExpressionCompiler),
                    _context.ShapeTable.GetShape(assignStmt.ExprAssignValue), _context);
            }
            else if (assignStmt.AssignTo is Indexer indexer)
            {
                gen.EmitAssign(indexer, () => assignStmt.ExprAssignValue.Accept(_context.ExpressionCompiler),
                    _context.ShapeTable.GetShape(assignStmt.ExprAssignValue), _context);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void VisitPostFixUnaryAssignStmt(PostFixUnaryAssignStmt postFixUnaryAssignStmt)
        {
            var gen = _context.CurrentEnv.Generator;
            var shape = _context.ShapeTable.GetShape(postFixUnaryAssignStmt.Left);

            void emitValue()
            {
                postFixUnaryAssignStmt.Left.Accept(_context.ExpressionCompiler);
                gen.EmitBoxIfNeeded(postFixUnaryAssignStmt, postFixUnaryAssignStmt.Left, _context.ShapeTable);

                if (shape.InferredType == typeof(double))
                {
                    gen.Emit(OpCodes.Ldc_R8, 1.0);
                    gen.Emit(postFixUnaryAssignStmt.Op.Type == Lexer.TokenType.Increment ? OpCodes.Add : OpCodes.Sub);
                }
                else
                {
                    gen.Emit(OpCodes.Ldc_R8, 1.0);
                    gen.Emit(OpCodes.Box, typeof(double));
                    _context.DynamicOperationsEmitted = true;
                    gen.EmitCall(OpCodes.Call, typeof(DynamicOperations)
                        .GetMethod(postFixUnaryAssignStmt.Op.Type == Lexer.TokenType.Increment ?
                            nameof(DynamicOperations.Add) : nameof(DynamicOperations.Subtract)), null);
                }
            }

            if (postFixUnaryAssignStmt.Left is MemberRootAccess rootAccess)
            {
                gen.EmitAssign(rootAccess, emitValue, shape, _context);
            }
            else if (postFixUnaryAssignStmt.Left is MemberAccess memberAccess)
            {
                gen.EmitAssign(memberAccess, emitValue, shape, _context);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
