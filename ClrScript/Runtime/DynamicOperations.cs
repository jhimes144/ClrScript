using ClrScript.Interop;
using ClrScript.Lexer.TokenReaders;
using ClrScript.Runtime.Builtins;
using ClrScript.TypeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClrScript.Runtime
{
    public class DynMethodInfo
    {
        public object Instance { get; }

        public MethodInfo Info { get; }

        public DynMethodInfo(object instance, MethodInfo info)
        {
            Instance = instance;
            Info = info;
        }
    }

    public static class DynamicOperations
    {
        [ThreadStatic]
        public static string _lastMemberAccessName;

        [ThreadStatic]
        public static Type _lastMemberAccessType;

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

            throw new ClrScriptRuntimeException($"Cannot add {left.GetClrScriptTypeDisplay()} with {right.GetClrScriptTypeDisplay()}.");
        }

        public static object Subtract(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD - rightD;
            }

            throw new ClrScriptRuntimeException($"Cannot subtract {left.GetClrScriptTypeDisplay()} with {right.GetClrScriptTypeDisplay()}.");
        }

        public static object Multiply(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD * rightD;
            }

            throw new ClrScriptRuntimeException($"Cannot multiply {left.GetClrScriptTypeDisplay()}  with  {right.GetClrScriptTypeDisplay()}.");
        }

        public static object Divide(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD / rightD;
            }

            throw new ClrScriptRuntimeException($"Cannot divide {left.GetClrScriptTypeDisplay()}  with  {right.GetClrScriptTypeDisplay()}.");
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

            throw new ClrScriptRuntimeException($"Cannot perform operator > on {left.GetClrScriptTypeDisplay()}  with  {right.GetClrScriptTypeDisplay()}.");
        }

        public static bool LessThan(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD < rightD;
            }

            throw new ClrScriptRuntimeException($"Cannot perform operator < on {left.GetClrScriptTypeDisplay()}  with  {right.GetClrScriptTypeDisplay()}.");
        }

        public static bool GreaterThanOrEqual(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD >= rightD;
            }

            throw new ClrScriptRuntimeException($"Cannot perform operator >= on {left.GetClrScriptTypeDisplay()}  with  {right.GetClrScriptTypeDisplay()}.");
        }

        public static bool LessThanOrEqual(object left, object right)
        {
            if (left is double leftD && right is double rightD)
            {
                return leftD <= rightD;
            }

            throw new ClrScriptRuntimeException($"Cannot perform operator <= on {left.GetClrScriptTypeDisplay()}   with   {right.GetClrScriptTypeDisplay()}.");
        }

        public static object UnaryMinus(object value)
        {
            if (value is double d)
            {
                return -d;
            }

            throw new ClrScriptRuntimeException($"Cannot perform unary operator - on {value.GetClrScriptTypeDisplay()}.");
        }

        public static object UnaryBang(object value)
        {
            if (value is bool b)
            {
                return !b;
            }

            throw new ClrScriptRuntimeException($"Cannot perform unary operator ! on {value.GetClrScriptTypeDisplay()}.");
        }

        public static void Assign(object instance, string memberName, object value)
        {
            var type = instance.GetType();

            if (type == typeof(ClrScriptObject))
            {
                ((ClrScriptObject)value).DynSet(memberName, value);
            }
            else if (type.IsValueType || type == typeof(string))
            {
                throw new ClrScriptRuntimeException($"Cannot perform assignment on {type}.");
            }
            else
            {
                throw new NotImplementedException("External type reflection assign.");
            }
        }

        public static object Indexer(object instance, object index, TypeManager typeManager)
        {
            var type = instance.GetType();
            var indexer = typeManager.GetTypeInfo(type)?.GetIndexer();

            if (indexer == null)
            {
                throw new ClrScriptRuntimeException($"Indexing is not possible on {type.GetClrScriptTypeDisplay()}.");
            }

            throw new NotImplementedException();
        }

        public static object MemberAccess(object instance, string memberName, TypeManager typeManager)
        {
            var type = instance.GetType();

            if (instance is ClrScriptObject clrObj)
            {
                return clrObj.DynGet(memberName);
            }

            var typeInfo = typeManager.GetTypeInfo(type);

            _lastMemberAccessName = memberName;
            _lastMemberAccessType = type;

            if (typeInfo != null)
            {
                var member = typeInfo.GetMember(memberName);

                if (member is FieldInfo field)
                {
                    return field.GetValue(instance);
                }

                if (member is PropertyInfo property)
                {
                    return property.GetValue(instance);
                }

                if (member is MethodInfo method)
                {
                    return new DynMethodInfo(instance, method);
                }
            }

            return null;
        }

        public static object Call(object methodData, object[] args)
        {
            if (methodData is Delegate del)
            {
                try
                {
                    CheckArgs(args, del.GetMethodInfo().GetParameters());
                    return del.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    throw new ClrScriptRuntimeException
                        (e, $"Error calling lambda method. {e.Message}");
                }
            }
            else if (methodData is DynMethodInfo dynMethodInfo)
            {
                try
                {
                    if (!dynMethodInfo.Info.IsStatic)
                    {
                        CheckArgs(args, dynMethodInfo.Info.GetParameters());
                        return dynMethodInfo.Info.Invoke(dynMethodInfo.Instance, args);
                    }
                    else
                    {
                        // method is a clr script extension. Otherwise validation would flag it.
                        var nArgs = new object[args.Length + 1];
                        nArgs[0] = dynMethodInfo.Instance;
                        args.CopyTo(nArgs, 1);

                        CheckArgs(nArgs, dynMethodInfo.Info.GetParameters());
                        return dynMethodInfo.Info.Invoke(null, nArgs);
                    }
                }
                catch (Exception e)
                {
                    throw new ClrScriptRuntimeException
                        (e, $"Error calling '{dynMethodInfo.Info.Name}'. {e.Message}");
                }
            }

            throw new ClrScriptRuntimeException($"'{_lastMemberAccessName}' is not callable on '{_lastMemberAccessType.GetClrScriptTypeDisplay()}'.");
        }

        static void CheckArgs(object[] args, ParameterInfo[] parameters)
        {
            if (args.Length != parameters.Length)
            {
                throw new ClrScriptRuntimeException($"Incorrect argument count supplied." +
                    $" Expected {parameters.Length} argument(s) and was supplied {args.Length}.");
            }

            for (var i = 0; i < args.Length; i++)
            {
                var argT = args[i].GetTypeIncludeNull();
                var parT = parameters[i].ParameterType;

                if (parT.IsAssignableFrom(argT))
                {
                    continue;
                }

                if (args[i] is double v)
                {
                    if (InteropHelpers.GetIsSupportedNumericInteropType(parT))
                    {
                        args[i] = InteropHelpers.ConvertDynBoxNumeric(v, parT);
                        continue;
                    }

                    throw new ClrScriptRuntimeException($"Cannot convert number to '{parT.Name}'");
                }
            }
        }

        public static DynMethodInfo CreateDynCallInfo(object instance, string methodName, TypeManager typeManager)
        {
            var type = instance.GetTypeIncludeNull();
            var method = typeManager.GetTypeInfo(type).GetMember(methodName) as MethodInfo;

            if (method == null)
            {
                throw new ClrScriptRuntimeException($"'{methodName}' is not callable on '{type.GetClrScriptTypeDisplay()}'.");
            }

            return new DynMethodInfo(instance, method);
        }
    }
}
