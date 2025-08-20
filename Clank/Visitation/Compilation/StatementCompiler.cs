using Clank.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.Compilation
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
            foreach (var stmt in block.Statements)
            {
                stmt.Accept(this);
            }
        }

        public void VisitExprStmt(ExpressionStmt exprStmt)
        {
            exprStmt.Expression.Accept(_context.ExpressionCompiler);
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
            _context.CurrentEnv.Generator.Emit(OpCodes.Ret);
            _context.ReturnPrepped = true;
        }

        public void VisitVarStmt(VarStmt varStmt)
        {
            _context.CurrentEnv.DeclareVariable(varStmt);
            varStmt.Initializer.Accept(_context.ExpressionCompiler);
            _context.CurrentEnv.VariableEmitStoreFromEvalStack(varStmt.Name.Value);
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
            forStmt.Increment?.Accept(_context.ExpressionCompiler);

            // Jump back to condition
            generator.Emit(OpCodes.Br, loopStart);
            
            // End of loop
            generator.MarkLabel(loopEnd);
        }
    }
}
