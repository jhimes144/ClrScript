using Clank.Elements.Expressions;
using Clank.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.Analyzer
{
    class AnalyzerVisitor : IStatementVisitor, IExpressionVisitor
    {
        readonly SymbolTable _symbolTable;

        public AnalyzerVisitor(SymbolTable symbolTable)
        {
            _symbolTable = symbolTable;
        }

        public void VisitAssign(Assign expr)
        {
        }

        public void VisitBinary(Binary expr)
        {
        }

        public void VisitBlock(Block block)
        {
        }

        public void VisitBlockExpr(BlockExpr blockExpr)
        {
        }

        public void VisitBlueprintPropStmt(BlueprintPropertyStmt blueprintPropStmt)
        {
        }

        public void VisitBlueprintStmt(BlueprintStmt blueprintStmt)
        {
        }

        public void VisitCall(Call call)
        {
        }

        public void VisitExprStmt(ExpressionStmt exprStmt)
        {
        }

        public void VisitForStmt(ForStmt forStmt)
        {
        }

        public void VisitGrouping(Grouping expr)
        {
        }

        public void VisitIfStmt(IfStmt ifStmt)
        {
        }

        public void VisitLambda(Lambda lambda)
        {
        }

        public void VisitLiteral(Literal expr)
        {
            if (expr.Value is bool b)
            {
                
            }
        }

        public void VisitLogical(Logical logical)
        {
        }

        public void VisitMemberAccess(MemberAccess memberAccess)
        {
        }

        public void VisitMemberRootAccess(MemberRootAccess member)
        {
        }

        public void VisitObjectLiteral(ObjectLiteral objLiteral)
        {
        }

        public void VisitReturnStmt(ReturnStmt returnStmt)
        {
        }

        public void VisitUnary(Unary expr)
        {
            expr.Right.Accept(this);

            // check to make sure unary operator can be one on expression type
        }

        public void VisitVarStmt(VarStmt varStmt)
        {
        }

        public void VisitWhileStmt(WhileStmt whileStmt)
        {
            whileStmt.Condition.Accept(this);
            whileStmt.Body.Accept(this);
        }
    }
}
