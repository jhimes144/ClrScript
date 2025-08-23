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
            var leftType = left.GetTypeIncludeNull();
            var rightType = right.GetTypeIncludeNull();

            if (leftType == typeof(double)
                   && rightType == typeof(double))
            {
                return (double)left + (double)right;
            }

            if (leftType == typeof(string) && rightType == typeof(string))
            {
                return (string)left + (string)right;
            }

            if (leftType == typeof(string))
            {
                return (string)left + right;
            }

            if (rightType == typeof(string))
            {
                return left + (string)right;
            }

            throw new ClankRuntimeException($"Cannot add {left.GetType()} with {right.GetType()}.");
        }

        public static object Subtract(object left, object right)
        {
            if (left.GetTypeIncludeNull() == typeof(double)
                && right.GetTypeIncludeNull() == typeof(double))
            {
                return (double)left - (double)right;
            }

            throw new ClankRuntimeException($"Cannot subtract {left.GetType()} with {right.GetType()}.");
        }

        public static object Multiply(object left, object right)
        {
            if (left.GetTypeIncludeNull() == typeof(double)
                && right.GetTypeIncludeNull() == typeof(double))
            {
                return (double)left * (double)right;
            }

            throw new ClankRuntimeException($"Cannot multiply {left.GetType()} with {right.GetType()}.");
        }

        public static object Divide(object left, object right)
        {
            if (left.GetTypeIncludeNull() == typeof(double)
                && right.GetTypeIncludeNull() == typeof(double))
            {
                return (double)left / (double)right;
            }

            throw new ClankRuntimeException($"Cannot divide {left.GetType()} with {right.GetType()}.");
        }

        public static bool EqualEqual(object left, object right)
        {
            var leftType = left.GetTypeIncludeNull();
            var rightType = right.GetTypeIncludeNull();

            if (leftType == typeof(double)
                && rightType == typeof(double))
            {
                return (double)left == (double)right;
            }

            if (leftType == typeof(bool)
                && rightType == typeof(bool))
            {
                return (bool)left == (bool)right;
            }

            if (leftType == typeof(string)
                && rightType == typeof(string))
            {
                return (string)left == (string)right;
            }

            return left == right;
        }

        public static object GreaterThan(object left, object right)
        {
            var leftType = left.GetTypeIncludeNull();
            var rightType = right.GetTypeIncludeNull();

            if (leftType == typeof(double) && rightType == typeof(double))
            {
                return (double)left > (double)right;
            }

            throw new ClankRuntimeException($"Cannot perform operator > on {left.GetType()} with {right.GetType()}.");
        }

        public static object LessThan(object left, object right)
        {
            var leftType = left.GetTypeIncludeNull();
            var rightType = right.GetTypeIncludeNull();

            if (leftType == typeof(double) && rightType == typeof(double))
            {
                return (double)left < (double)right;
            }

            throw new ClankRuntimeException($"Cannot perform operator < on {left.GetType()} with {right.GetType()}.");
        }

        public static object GreaterThanOrEqual(object left, object right)
        {
            var leftType = left.GetTypeIncludeNull();
            var rightType = right.GetTypeIncludeNull();

            if (leftType == typeof(double) && rightType == typeof(double))
            {
                return (double)left >= (double)right;
            }

            throw new ClankRuntimeException($"Cannot perform operator >= on {left.GetType()} with {right.GetType()}.");
        }

        public static object LessThanOrEqual(object left, object right)
        {
            var leftType = left.GetTypeIncludeNull();
            var rightType = right.GetTypeIncludeNull();

            if (leftType == typeof(double) && rightType == typeof(double))
            {
                return (double)left <= (double)right;
            }

            throw new ClankRuntimeException($"Cannot perform operator <= on {left.GetType()} with {right.GetType()}.");
        }

        public static object UnaryMinus(object value)
        {
            if (value.GetTypeIncludeNull() == typeof(double))
            {
                return -(double)value;
            }

            throw new ClankRuntimeException($"Cannot perform unary operator - on {value.GetTypeIncludeNull()}.");
        }

        public static object UnaryBang(object value)
        {
            if (value.GetTypeIncludeNull() == typeof(bool))
            {
                return !(bool)value;
            }

            throw new ClankRuntimeException($"Cannot perform unary operator ! on {value.GetTypeIncludeNull()}.");
        }
    }
}
