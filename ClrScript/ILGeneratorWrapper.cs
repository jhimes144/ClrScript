using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ClrScript.Elements;
using ClrScript.Elements.Expressions;
using ClrScript.Visitation;
using System.Collections;
using ClrScript.Interop;
using ClrScript.Runtime;
using ClrScript.Elements.Statements;
using ClrScript.Visitation.Compilation;
using ClrScript.TypeManagement;

namespace ClrScript
{
    class ILOpDisplay
    {
        public OpCode Code { get; }

        public object Argument { get; }

        public ILOpDisplay(OpCode code, object argument)
        {
            Code = code;
            Argument = argument;
        }

        public override string ToString()
        {
            var code = Code.Name.Replace('.', '_');

            if (Argument == null)
            {
                return code;
            }
            else
            {
                return $"{code} {Argument}";
            }
        }
    }

    class ILGeneratorWrapper
    {
        readonly ConstructorInfo _clrExcepMessageCstruc = typeof(ClrScriptRuntimeException)
            .GetConstructor(new[] { typeof(string) });

        readonly ILGenerator _ilGenerator;
        readonly List<ILOpDisplay> _instructionsRendered = new List<ILOpDisplay>();

        public IReadOnlyList<ILOpDisplay> InstructionsRendered => _instructionsRendered;

        public Stack CompilerStack { get; } = new Stack();

        public ILGeneratorWrapper(ILGenerator ilGenerator)
        {
            _ilGenerator = ilGenerator ?? throw new ArgumentNullException(nameof(ilGenerator));
        }

        public virtual void Emit(OpCode opcode)
        {
            _ilGenerator.Emit(opcode);
            _instructionsRendered.Add(new ILOpDisplay(opcode, null));
        }

        public virtual void Emit(OpCode opcode, byte arg)
        {
            _ilGenerator.Emit(opcode, arg);
            _instructionsRendered.Add(new ILOpDisplay(opcode, arg));
        }

        public virtual void Emit(OpCode opcode, short arg)
        {
            _ilGenerator.Emit(opcode, arg);
            _instructionsRendered.Add(new ILOpDisplay(opcode, arg));
        }

        public virtual void Emit(OpCode opcode, long arg)
        {
            _ilGenerator.Emit(opcode, arg);
            _instructionsRendered.Add(new ILOpDisplay(opcode, arg));
        }

        public virtual void Emit(OpCode opcode, float arg)
        {
            _ilGenerator.Emit(opcode, arg);
            _instructionsRendered.Add(new ILOpDisplay(opcode, arg));
        }

        public virtual void Emit(OpCode opcode, double arg)
        {
            _ilGenerator.Emit(opcode, arg);
            _instructionsRendered.Add(new ILOpDisplay(opcode, arg));
        }

        public virtual void Emit(OpCode opcode, int arg)
        {
            _ilGenerator.Emit(opcode, arg);
            _instructionsRendered.Add(new ILOpDisplay(opcode, arg));
        }

        public virtual void Emit(OpCode opcode, MethodInfo meth)
        {
            _ilGenerator.Emit(opcode, meth);
            _instructionsRendered.Add(new ILOpDisplay(opcode, meth));
        }

        public virtual void EmitCalli(OpCode opcode, CallingConventions callingConvention,
            Type? returnType, Type[]? parameterTypes, Type[]? optionalParameterTypes)
        {
            _ilGenerator.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
            _instructionsRendered.Add(new ILOpDisplay(opcode, new { callingConvention, returnType, parameterTypes, optionalParameterTypes }));
        }

        public virtual void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type? returnType, Type[]? parameterTypes)
        {
            _ilGenerator.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
            _instructionsRendered.Add(new ILOpDisplay(opcode, new { unmanagedCallConv, returnType, parameterTypes }));
        }

        public virtual void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[]? optionalParameterTypes)
        {
            _ilGenerator.EmitCall(opcode, methodInfo, optionalParameterTypes);
            _instructionsRendered.Add(new ILOpDisplay(opcode, new { methodInfo, optionalParameterTypes }));
        }

        public virtual void Emit(OpCode opcode, SignatureHelper signature)
        {
            _ilGenerator.Emit(opcode, signature);
            _instructionsRendered.Add(new ILOpDisplay(opcode, signature));
        }

        public virtual void Emit(OpCode opcode, ConstructorInfo con)
        {
            _ilGenerator.Emit(opcode, con);
            _instructionsRendered.Add(new ILOpDisplay(opcode, con));
        }

        public virtual void Emit(OpCode opcode, Type cls)
        {
            _ilGenerator.Emit(opcode, cls);
            _instructionsRendered.Add(new ILOpDisplay(opcode, cls));
        }

        public virtual void Emit(OpCode opcode, Label label)
        {
            _ilGenerator.Emit(opcode, label);
            _instructionsRendered.Add(new ILOpDisplay(opcode, label));
        }

        public virtual void Emit(OpCode opcode, Label[] labels)
        {
            _ilGenerator.Emit(opcode, labels);
            _instructionsRendered.Add(new ILOpDisplay(opcode, labels));
        }

        public virtual void Emit(OpCode opcode, FieldInfo field)
        {
            _ilGenerator.Emit(opcode, field);
            _instructionsRendered.Add(new ILOpDisplay(opcode, field));
        }

        public virtual void Emit(OpCode opcode, string str)
        {
            _ilGenerator.Emit(opcode, str);
            _instructionsRendered.Add(new ILOpDisplay(opcode, str));
        }

        public virtual void Emit(OpCode opcode, LocalBuilder local)
        {
            _ilGenerator.Emit(opcode, local);
            _instructionsRendered.Add(new ILOpDisplay(opcode, local));
        }

        public virtual Label DefineLabel()
        {
            return _ilGenerator.DefineLabel();
        }

        public virtual void MarkLabel(Label loc)
        {
            _ilGenerator.MarkLabel(loc);
        }

        public virtual LocalBuilder DeclareLocal(Type localType)
        {
            return _ilGenerator.DeclareLocal(localType);
        }

        public virtual LocalBuilder DeclareLocal(Type localType, bool pinned)
        {
            return _ilGenerator.DeclareLocal(localType, pinned);
        }

        public virtual void BeginExceptionBlock()
        {
            _ilGenerator.BeginExceptionBlock();
        }

        public virtual void EndExceptionBlock()
        {
            _ilGenerator.EndExceptionBlock();
        }

        public virtual void BeginCatchBlock(Type? exceptionType)
        {
            _ilGenerator.BeginCatchBlock(exceptionType);
        }

        public virtual void BeginFinallyBlock()
        {
            _ilGenerator.BeginFinallyBlock();
        }

        public virtual void BeginFaultBlock()
        {
            _ilGenerator.BeginFaultBlock();
        }

        public virtual void BeginScope()
        {
            _ilGenerator.BeginScope();
        }

        public virtual void EndScope()
        {
            _ilGenerator.EndScope();
        }

        public virtual void UsingNamespace(string usingNamespace)
        {
            _ilGenerator.UsingNamespace(usingNamespace);
        }

        public void EmitBoxIfNeeded(Element currentElement, Element previousElement, ShapeTable shapeTable)
        {
            var currentShapeInfo = shapeTable.GetShape(currentElement);
            var previousShapeInfo = shapeTable.GetShape(previousElement);

            EmitBoxIfNeeded(currentShapeInfo, previousShapeInfo);
        }

        public void EmitBoxIfNeeded(ShapeInfo currentShape, ShapeInfo previousShape)
        {
            var currentShapeT = currentShape?.InferredType ?? typeof(object);
            var previousShapeT = previousShape?.InferredType ?? typeof(object);

            if (!currentShapeT.IsValueType && previousShapeT.IsValueType)
            {
                if (InteropHelpers.GetIsSupportedNumericInteropTypeNeedingConversion(previousShapeT))
                {
                    // emit from before would of already converted the value.
                    previousShapeT = typeof(double);
                }

                Emit(OpCodes.Box, previousShapeT);
            }
        }

        public void EmitDynamicCall(Call call, IExpressionVisitor visitor, ShapeTable shapeTable)
        {
            Emit(OpCodes.Ldc_I4, call.Arguments.Count);
            Emit(OpCodes.Newarr, typeof(object));

            for (int i = 0; i < call.Arguments.Count; i++)
            {
                Emit(OpCodes.Dup);
                Emit(OpCodes.Ldc_I4, i);

                call.Arguments[i].Accept(visitor);
                EmitBoxIfNeeded(call, call.Arguments[i], shapeTable);

                Emit(OpCodes.Stelem_Ref);
            }

            EmitCall(OpCodes.Call, typeof(DynamicOperations)
                .GetMethod(nameof(DynamicOperations.Call)), null);
        }

        public void EmitMemberAccess(ShapeInfo objShapeInfo, string memberName, ShapeInfo memberShapeInfo,
            CompilationContext context)
        {
            if (memberShapeInfo is UnknownShape || objShapeInfo is UnknownShape)
            {
                Emit(OpCodes.Ldstr, memberName);
                Emit(OpCodes.Ldarg_2); // type manager
                EmitCall(OpCodes.Call, typeof(DynamicOperations)
                        .GetMethod(nameof(DynamicOperations.MemberAccess)), null);

                context.DynamicOperationsEmitted = true;
                return;
            }

            var parentType = objShapeInfo.InferredType;
            var typeInfo = context.TypeManager.GetTypeInfo(parentType);

            if (typeInfo != null)
            {
                var member = typeInfo.GetMember(memberName);

                if (member is FieldInfo field)
                {
                    Emit(OpCodes.Ldfld, field);
                    return;
                }

                if (member is PropertyInfo property)
                {
                    Emit(OpCodes.Callvirt, property.GetGetMethod());
                    return;
                }

                if (member is MethodInfo method)
                {
                    CompilerStack.Push(method);
                    return;
                }
            }

            Emit(OpCodes.Ldstr, memberName);
            Emit(OpCodes.Ldarg_2); // type manager
            EmitCall(OpCodes.Call, typeof(DynamicOperations)
                .GetMethod(nameof(DynamicOperations.MemberAccess)), null);

            context.DynamicOperationsEmitted = true;
        }

        public void EmitAssign(MemberRootAccess rootAccess, Action emitValue, ShapeInfo valueShape, CompilationContext context)
        {
            if (rootAccess.AccessType == RootMemberAccessType.Variable)
            {
                emitValue();
                EmitBoxIfNeeded(context.ShapeTable.GetShape(rootAccess), valueShape);

                context.CurrentEnv.VariableEmitStoreFromEvalStack(rootAccess.Name.Value);
            }
            else if (rootAccess.AccessType == RootMemberAccessType.External)
            {
                var typeInfo = context.TypeManager.GetTypeInfo(context.InType);
                var member = typeInfo.GetMember(rootAccess.Name.Value);

                EmitAssign(emitValue, () => Emit(OpCodes.Ldarg_1),
                    member, valueShape);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void EmitAssign(MemberAccess memberAccess, Action emitValue, ShapeInfo valueShape, CompilationContext context)
        {
            var assigneeShape = context.ShapeTable.GetShape(memberAccess.Expr);
            MemberInfo member = null;

            if (assigneeShape.InferredType.GetField(memberAccess.Name.Value) is FieldInfo field)
            {
                member = field;
            }
            else if (assigneeShape.InferredType.GetProperty(memberAccess.Name.Value) is PropertyInfo prop)
            {
                member = prop;
            }

            EmitAssign(emitValue, () => memberAccess.Expr.Accept(context.ExpressionCompiler),
                member, valueShape);
        }

        public void EmitAssign(Action emitValue,
            Action emitLdAssigneObject, MemberInfo member, ShapeInfo valueShape)
        {
            Type memberAssignType;
            PropertyInfo prop = null;
            FieldInfo field = null;

            if (member is PropertyInfo propP)
            {
                memberAssignType = propP.PropertyType;
                prop = propP;
            }
            else if (member is FieldInfo fieldP)
            {
                memberAssignType = fieldP.FieldType;
                field = fieldP;
            }
            else
            {
                throw new NotSupportedException("Invalid member type, must be field or property.");
            }

            var memberAssignIsSupportedNum = InteropHelpers.GetIsSupportedNumericInteropType(memberAssignType);
            var expressionType = valueShape?.InferredType ?? typeof(object);

            if (expressionType == memberAssignType)
            {
                // direct assign, easy and fast
                emitLdAssigneObject();
                emitValue();

                if (prop != null)
                {
                    Emit(OpCodes.Callvirt, prop.GetSetMethod());
                }
                else
                {
                    Emit(OpCodes.Stfld, field);
                }
            }
            else
            {
                if (expressionType == typeof(double) && memberAssignIsSupportedNum)
                {
                    emitLdAssigneObject();
                    emitValue();
                    EmitStackDoubleToNumericValueTypeConversion(memberAssignType);

                    if (prop != null)
                    {
                        Emit(OpCodes.Callvirt, prop.GetSetMethod());
                    }
                    else
                    {
                        Emit(OpCodes.Stfld, field);
                    }
                }
                else if (!expressionType.IsValueType)
                {
                    var lblEnd = DefineLabel();
                    var lblFailure = DefineLabel();

                    emitLdAssigneObject();
                    emitValue();

                    if (memberAssignIsSupportedNum)
                    {
                        // Stack: [instance, value]
                        Emit(OpCodes.Dup); // Stack: [instance, value, value]
                        Emit(OpCodes.Call, typeof(object).GetMethod("GetType")); // Stack: [instance, value, type]
                        Emit(OpCodes.Ldtoken, typeof(double)); // Stack: [instance, value, type, token]
                        Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) })); // Stack: [instance, value, type, type]
                        Emit(OpCodes.Call, typeof(Type).GetMethod("op_Equality", new Type[] { typeof(Type), typeof(Type) })); // Stack: [instance, value, bool]
                        Emit(OpCodes.Brfalse_S, lblFailure); // Stack: [instance, value]

                        Emit(OpCodes.Unbox_Any, typeof(double)); // Stack: [instance, double]
                        EmitStackDoubleToNumericValueTypeConversion(memberAssignType); // Stack: [instance, converted_value]

                        if (prop != null)
                        {
                            Emit(OpCodes.Callvirt, prop.GetSetMethod());
                        }
                        else
                        {
                            Emit(OpCodes.Stfld, field);
                        }

                        Emit(OpCodes.Br, lblEnd);
                    }
                    else
                    {
                        // Stack: [instance, value]
                        Emit(OpCodes.Dup); // Stack: [instance, value, value]
                        Emit(OpCodes.Call, typeof(object).GetMethod("GetType")); // Stack: [instance, value, type]
                        Emit(OpCodes.Ldtoken, memberAssignType); // Stack: [instance, value, type, token]
                        Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) })); // Stack: [instance, value, type, type]
                        Emit(OpCodes.Call, typeof(Type).GetMethod("op_Equality", new Type[] { typeof(Type), typeof(Type) })); // Stack: [instance, value, bool]
                        Emit(OpCodes.Brfalse_S, lblFailure); // Stack: [instance, value]

                        if (memberAssignType.IsValueType)
                        {
                            Emit(OpCodes.Unbox_Any, memberAssignType); // Stack: [instance, unboxed_value]
                        }
                        else
                        {
                            Emit(OpCodes.Castclass, memberAssignType); // Stack: [instance, cast_value]
                        }

                        if (prop != null)
                        {
                            Emit(OpCodes.Callvirt, prop.GetSetMethod());
                        } 
                        else
                        {
                            Emit(OpCodes.Stfld, field);
                        }

                        Emit(OpCodes.Br, lblEnd);
                    }

                    MarkLabel(lblFailure);
                    // Stack: [instance, value]
                    Emit(OpCodes.Pop); // pop value - Stack: [instance]
                    Emit(OpCodes.Pop); // pop instance - Stack: []
                    EmitThrowClrRuntimeException($"Cannot assign to '{member.Name}'. Data is in wrong format. " +
                        $"Expected '{memberAssignType.Name}'.");

                    MarkLabel(lblEnd);
                }
                else
                {
                    EmitThrowClrRuntimeException($"Cannot assign value of type " +
                        $"'{expressionType.Name}' to '{member.Name}' of type '{memberAssignType.Name}'.");
                }
            }
        }

        public T ConsumeCompilerStack<T>() where T : class
        {
            var pop = CompilerStack.Pop() as T;

            if (pop == null)
            {
                throw new Exception($"Was expecting {typeof(T).Name} on compiler stack.");
            }

            return pop;
        }

        public void EmitThrowClrRuntimeException(string message)
        {
            Emit(OpCodes.Ldstr, message);
            Emit(OpCodes.Newobj, _clrExcepMessageCstruc);
            Emit(OpCodes.Throw);
        }

        public void EmitStackDoubleToNumericValueTypeConversion(Type targetType)
        {
            if (targetType == typeof(int))
            {
                Emit(OpCodes.Conv_I4);
            }
            else if (targetType == typeof(long))
            {
                Emit(OpCodes.Conv_I8);
            }
            else if (targetType == typeof(short))
            {
                Emit(OpCodes.Conv_I2);
            }
            else if (targetType == typeof(uint))
            {
                Emit(OpCodes.Conv_U4);
            }
            else if (targetType == typeof(ulong))
            {
                Emit(OpCodes.Conv_U8);
            }
            else if (targetType == typeof(ushort))
            {
                Emit(OpCodes.Conv_U2);
            }
            else if (targetType == typeof(byte))
            {
                Emit(OpCodes.Conv_U1);
            }
            else if (targetType == typeof(sbyte))
            {
                Emit(OpCodes.Conv_I1);
            }
            else if (targetType == typeof(float))
            {
                Emit(OpCodes.Conv_R4);
            }
            else if (targetType == typeof(decimal))
            {
                Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDecimal", new[] { typeof(double) }));
            }
            else
            {
                // this should not happen, external type analyzer should throw for it.
                throw new NotSupportedException
                    ($"Unsupported target type for numeric conversion: {targetType.Name}");
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _instructionsRendered);
        }
    }
}
