using ClrScript.Elements;
using ClrScript.Elements.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ClrScript.Visitation.Compilation
{
    static class CompileHelpers
    {
        public static bool GetCanBeOptimized(MethodInfo methodInfo, Call call, ShapeTable shapeTable)
        {
            var args = methodInfo.GetParameters();

            if (args.Length != call.Arguments.Count)
            {
                return false;
            }

            for (var i = 0; i < args.Length; i++)
            {
                var argShape = shapeTable.GetShape(call.Arguments[i]);

                if (argShape is UnknownShape)
                {
                    return false;
                }

                var methodArgType = args[i].ParameterType;

                if (!methodArgType.IsAssignableFrom(argShape.InferredType))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool GetCanBeOptimized(Type delType, Call call, ShapeTable shapeTable)
        {
            if (!typeof(Delegate).IsAssignableFrom(delType))
            {
                return false;
            }

            var invokeMethod = delType.GetMethod("Invoke");

            if (invokeMethod == null)
            {
                return false;
            }

            return GetCanBeOptimized(invokeMethod, call, shapeTable);
        }
    }
}
