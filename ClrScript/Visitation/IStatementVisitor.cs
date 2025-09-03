using ClrScript.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation
{
    interface IStatementVisitor
    {
        void VisitBlock(Block block);
        void VisitVarStmt(VarStmt varStmt);
        void VisitIfStmt(IfStmt ifStmt);
        void VisitExprStmt(ExpressionStmt exprStmt);
        void VisitWhileStmt(WhileStmt whileStmt);
        void VisitReturnStmt(ReturnStmt returnStmt);
        void VisitForStmt(ForStmt forStmt);
        void VisitPrintStmt(PrintStmt printStmt);
        void VisitAssignStmt(AssignStmt assignStmt);
    }
}
