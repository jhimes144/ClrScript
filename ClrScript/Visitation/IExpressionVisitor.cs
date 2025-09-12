using ClrScript.Elements.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation
{
    interface IExpressionVisitor
    {
        void VisitBinary(Binary expr);
        void VisitGrouping(Grouping expr);
        void VisitLiteral(Literal expr);
        void VisitUnary(Unary expr);
        void VisitMemberRootAccess(MemberRootAccess member);
        void VisitLogical(Logical logical);
        void VisitLambda(Lambda lambda);
        void VisitBlockExpr(BlockExpr blockExpr);
        void VisitObjectLiteral(ObjectLiteral objLiteral);
        void VisitMemberAccess(MemberAccess memberAccess);
        void VisitCall(Call call);
        void VisitInterpolatedString(InterpolatedStr str);
        void VisitArrayLiteral(ArrayLiteral expr);
        void VisitIndexer(Indexer indexer);
    }
}
