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
        readonly Dictionary<(TokenType op, Type left, Type right), Type> _operatorTable 
            = new Dictionary<(TokenType op, Type left, Type right), Type>();

        public OperatorResolver(ClankCompilationSettings settings)
        {
            var numberType = settings.NumberPrecision == NumberPrecision.DoublePrecision 
                ? typeof(double) : typeof(float);

            var boolType = typeof(bool);
            var stringType = typeof(string);
            var objType = typeof(object);

            addOperator(TokenType.Plus, numberType, numberType, numberType);
            addOperator(TokenType.Plus, stringType, stringType, stringType);

            addOperator(TokenType.Divide, numberType, numberType, numberType);
        }

        void addOperator(TokenType op, Type left, Type right, Type result)
        {
            _operatorTable[(op, left, right)] = result;
        }

        public Type ResolveOperator(TokenType op, Type leftType, Type rightType)
        {
            if (_operatorTable.TryGetValue((op, leftType, rightType), out Type result))
            {
                return result;
            }

            if (op == TokenType.EqualEqual || op == TokenType.BangEqual)
            {
                // we use object.Equals when compiling. We do not enforce the same restrictions as c#
                return typeof(bool);
            }

            return null;
        }
    }
}
