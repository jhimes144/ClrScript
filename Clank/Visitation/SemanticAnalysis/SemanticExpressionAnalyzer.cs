using Clank.Elements;
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

            var leftType = _context.SymbolTable.TryGetSymbolFor(expr.Left).Type;
            var rightType = _context.SymbolTable.TryGetSymbolFor(expr.Right).Type;

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
            var grpType = _context.SymbolTable.TryGetSymbolFor(expr.Expression).Type;
            _context.SymbolTable.SetType(expr, grpType);
        }

        public void VisitLambda(Lambda lambda)
        {
        }

        public void VisitLiteral(Literal expr)
        {
            if (expr.Value != null)
            {
                if (expr.Value is bool)
                {
                    _context.SymbolTable.SetType(expr, ClankTypeMeta.Bool);
                }
                else if (expr.Value is double || expr.Value is float)
                {
                    _context.SymbolTable.SetType(expr, ClankTypeMeta.Number);
                }
                else if (expr.Value is string)
                {
                    _context.SymbolTable.SetType(expr, ClankTypeMeta.String);
                }
            }
        }

        public void VisitLogical(Logical expr)
        {
            expr.Left.Accept(this);
            expr.Right.Accept(this);

            var leftType = _context.SymbolTable.TryGetSymbolFor(expr.Left).Type;
            var rightType = _context.SymbolTable.TryGetSymbolFor(expr.Right).Type;

            var opType = _context.OperatorResolver.ResolveOperator(expr.Op.Type, leftType, rightType);

            if (opType == null)
            {
                _context.Errors.Add(new ClankCompileException
                    ($"Cannot perform operator '{expr.Op.Value}' with '{leftType}' and '{rightType}'.", expr.Op));
            }

            _context.SymbolTable.SetType(expr, opType);
        }

        public void VisitPropertyAccess(PropertyAccess memberAccess)
        {
            memberAccess.Expr.Accept(this);
            var exprType = _context.SymbolTable.TryGetSymbolFor(memberAccess.Expr).Type;

            if (exprType.IsExternal)
            {
                var externalType = _context.ExternalTypes.GetMetaForTypeMemberByName
                    (exprType.ExternalType, memberAccess.Name.Value);

                if (externalType != null)
                {
                    _context.SymbolTable.SetType(memberAccess, externalType);
                    return;
                }
            }
            else
            {
                // todo: get type created by script user
            }

            _context.Errors.Add(new ClankCompileException
                ($"'{exprType}' does not contain a member called '{memberAccess.Name.Value}'."));
        }

        public void VisitCall(Call call)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
        }

        public void VisitUnary(Unary expr)
        {
        }

        public void VisitVariable(Variable var)
        {
            // a variable will either point to a local variable
            // in our scope, or a member of TIn

            var declaration = _context.SymbolTable.CurrentScope.GetVarDeclaration(var.Name.Value);

            if (declaration != null)
            {
                var varType = _context.SymbolTable.TryGetSymbolFor(declaration).Type;
                _context.SymbolTable.SetType(var, varType);
                return;
            }
            else
            {
                // this variable has now become effectively a member access to our In Type instance

                var externalType = _context.ExternalTypes.GetMetaForInTypeMemberByName(var.Name.Value);

                if (externalType != null)
                {
                    _context.SymbolTable.SetType(var, externalType);
                    return;
                }
            }

            _context.Errors.Add(new ClankCompileException
                    ($"The name '{var.Name.Value}' does not exist in the current context."));
        }
    }
}
