using Clank.Elements.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.SemanticAnalysis
{
    internal class SemanticExpressionAnalyzer : IExpressionVisitor
    {
        readonly AnalysisContext _context;

        public SemanticExpressionAnalyzer(AnalysisContext context)
        {
            _context = context;
        }

        public void VisitAssign(Assign expr)
        {
        }

        public void VisitBinary(Binary expr)
        {
            expr.Left.Accept(this);
            expr.Right.Accept(this);

            var leftType = _context.SymbolTable.GetSymbolFor(expr.Left).Type;
            var rightType = _context.SymbolTable.GetSymbolFor(expr.Right).Type;

            var opType = _context.OperatorResolver.ResolveOperator(expr.Op.Type, leftType, rightType);

            if (opType == null)
            {
                _context.Errors.Add(new ClankCompileException
                    ($"Cannot perform operator '{expr.Op.Value}' with '{leftType}' and '{rightType}'.", expr.Op));
            }

            _context.SymbolTable.SetType(expr, opType);
        }

        public void VisitBlockExpr(BlockExpr blockExpr)
        {
        }

        public void VisitGrouping(Grouping expr)
        {
            expr.Expression.Accept(this);
            var grpType = _context.SymbolTable.GetSymbolFor(expr.Expression).Type;
            _context.SymbolTable.SetType(expr, grpType);
        }

        public void VisitLambda(Lambda lambda)
        {
        }

        public void VisitLiteral(Literal expr)
        {
            if (expr.Value != null)
            {
                _context.SymbolTable.SetType(expr, expr.Value.GetType());
            }
        }

        public void VisitLogical(Logical expr)
        {
            expr.Left.Accept(this);
            expr.Right.Accept(this);

            var leftType = _context.SymbolTable.GetSymbolFor(expr.Left).Type;
            var rightType = _context.SymbolTable.GetSymbolFor(expr.Right).Type;

            var opType = _context.OperatorResolver.ResolveOperator(expr.Op.Type, leftType, rightType);

            if (opType == null)
            {
                _context.Errors.Add(new ClankCompileException
                    ($"Cannot perform operator '{expr.Op.Value}' with '{leftType}' and '{rightType}'.", expr.Op));
            }

            _context.SymbolTable.SetType(expr, opType);
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
        }

        public void VisitUnary(Unary expr)
        {
        }

        public void VisitVariable(Variable var)
        {
            var declaration = _context.SymbolTable.CurrentScope.GetVarDeclaration(var.Name.Value);

            if (declaration == null)
            {
                _context.Errors.Add(new ClankCompileException
                    ($"The name '{var.Name.Value}' does not exist in the current context."));
            }

            var varType = _context.SymbolTable.GetSymbolFor(declaration).Type;
            _context.SymbolTable.SetType(var, varType);
        }
    }
}
