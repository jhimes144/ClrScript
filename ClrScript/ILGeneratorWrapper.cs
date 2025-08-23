using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    public class ILOpDisplay
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

    public class ILGeneratorWrapper
    {
        readonly ILGenerator _ilGenerator;
        readonly List<ILOpDisplay> _instructionsRendered = new List<ILOpDisplay>();

        public IReadOnlyList<ILOpDisplay> InstructionsRendered => _instructionsRendered;

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

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _instructionsRendered);
        }
    }
}