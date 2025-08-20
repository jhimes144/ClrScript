using Clank.Elements;
using Clank.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.SemanticAnalysis
{
    class OperatorResolver
    {
        readonly Dictionary<(TokenType op, ClankTypeMeta left, ClankTypeMeta right), ClankTypeMeta> _operatorTable
            = new Dictionary<(TokenType op, ClankTypeMeta left, ClankTypeMeta right), ClankTypeMeta>();

        public OperatorResolver(ClankCompilationSettings settings)
        {
            var numberType = ClankTypeMeta.Number;
            var boolType = ClankTypeMeta.Bool;
            var stringType = ClankTypeMeta.String;

            addOperator(TokenType.Plus, numberType, numberType, numberType);
            addOperator(TokenType.Divide, numberType, numberType, numberType);
            addOperator(TokenType.Minus, numberType, numberType, numberType);
            addOperator(TokenType.Multiply, numberType, numberType, numberType);

            addOperator(TokenType.GreaterThan, numberType, numberType, boolType);
            addOperator(TokenType.LessThan, numberType, numberType, boolType);
            addOperator(TokenType.GreaterThanOrEqual, numberType, numberType, boolType);
            addOperator(TokenType.LessThanOrEqual, numberType, numberType, boolType);
        }

        void addOperator(TokenType op, ClankTypeMeta left, ClankTypeMeta right, ClankTypeMeta result)
        {
            _operatorTable[(op, left, right)] = result;
        }

        public ClankTypeMeta ResolveOperator(TokenType op, ClankTypeMeta leftType, ClankTypeMeta rightType)
        {
            if (_operatorTable.TryGetValue((op, leftType, rightType), out ClankTypeMeta result))
            {
                return result;
            }

            if (op == TokenType.EqualEqual || op == TokenType.BangEqual
                || op == TokenType.DoubleAmpersand || op == TokenType.DoublePipe)
            {
                return ClankTypeMeta.Bool;
            }

            return null;
        }
    }
}
