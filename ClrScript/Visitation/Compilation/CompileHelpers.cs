using ClrScript.Elements;
using ClrScript.Elements.Expressions;
using ClrScript.Interop;
using ClrScript.Visitation.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ClrScript.Visitation.Compilation
{
    static class CompileHelpers
    {
        public static bool GetCanBeOptimized(MethodInfo methodInfo, Call call, ShapeTable shapeTable, out ParameterInfo[] correctedParams)
        {
            var args = methodInfo.GetParameters();

            if (Util.IsExtensionMethod(methodInfo))
            {
                // method is an extension. we already know that the instance object
                // is of the correct shape, otherwise we wouldn't have a method info to work with.
                var nArgs = new ParameterInfo[args.Length - 1];
                args.AsSpan(1).CopyTo(nArgs);
                args = nArgs;
            }

            correctedParams = args;

            if (args.Length != call.Arguments.Count)
            {
                return false;
            }

            for (var i = 0; i < args.Length; i++)
            {
                var argShape = shapeTable.GetShape(call.Arguments[i]);

                if (argShape is OldUnknownShape)
                {
                    return false;
                }

                var methodArgType = args[i].ParameterType;

                if (!methodArgType.IsAssignableFrom(argShape.InferredType) 
                    && !InteropHelpers.GetIsSupportedNumericInteropType(methodArgType))
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

            return GetCanBeOptimized(invokeMethod, call, shapeTable, out _);
        }
    }
}
