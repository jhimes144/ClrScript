using Clank.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Runtime
{
    public static class Operators
    {
        public static object Add(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD + rightD;
            }

            if (left is string leftS)
            {
                if (right is string rightS)
                {
                    return leftS + rightS;
                }

                return leftS + right;
            }

            if (right is string rightSS)
            {
                return left + rightSS;
            }

            throw new ClankRuntimeException($"Cannot add {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static object Subtract(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD - rightD;
            }

            throw new ClankRuntimeException($"Cannot subtract {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static object Multiply(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD * rightD;
            }

            throw new ClankRuntimeException($"Cannot multiply {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static object Divide(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD / rightD;
            }

            throw new ClankRuntimeException($"Cannot divide {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static bool EqualEqual(object left, object right)
        {
            return (left, right) switch
            {
                (double leftD, double rightD) => leftD == rightD,
                (bool leftB, bool rightB) => leftB == rightB,
                (string leftS, string rightS) => leftS == rightS,
                _ => left == right
            };
        }

        public static bool GreaterThan(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD > rightD;
            }

            throw new ClankRuntimeException($"Cannot perform operator > on {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static bool LessThan(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD < rightD;
            }

            throw new ClankRuntimeException($"Cannot perform operator < on {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static bool GreaterThanOrEqual(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD >= rightD;
            }

            throw new ClankRuntimeException($"Cannot perform operator >= on {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static bool LessThanOrEqual(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD <= rightD;
            }

            throw new ClankRuntimeException($"Cannot perform operator <= on {left.GetTypeIncludeNull()} with {right.GetTypeIncludeNull()}.");
        }

        public static object UnaryMinus(object value)
        {
            if (value is double d)
            {
                return -d;
            }

            throw new ClankRuntimeException($"Cannot perform unary operator - on {value.GetTypeIncludeNull()}.");
        }

        public static object UnaryBang(object value)
        {
            if (value is bool b)
            {
                return !b;
            }

            throw new ClankRuntimeException($"Cannot perform unary operator ! on {value.GetTypeIncludeNull()}.");
        }

        public static void Assign(object instance, string memberName, object value)
        {
            var type = instance.GetTypeIncludeNull();

            if (type == typeof(ClankObject))
            {
                ((ClankObject)value).Set(memberName, value);
            }
            else if (type.IsValueType || type == typeof(string))
            {
                throw new ClankRuntimeException($"Cannot perform assignment on {type}.");
            }
            else
            {
                throw new NotImplementedException("External type reflection assign.");
            }
        }
    }
}