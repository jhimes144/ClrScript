using Clank.Elements.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation
{
    interface IExpressionVisitor
    {
        void VisitAssign(Assign expr);
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
    }
}

