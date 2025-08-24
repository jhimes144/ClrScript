using ClrScript.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Compilation
{
    abstract class ClrMethodEnvironment
    {
        public ILGeneratorWrapper Generator { get; protected set; }
        public MethodBuilder Builder { get; protected set; }

        readonly Dictionary<string, LocalBuilder> _localsByName 
            = new Dictionary<string, LocalBuilder>();

        public ClrMethodEnvironment(CompilationContext context)
        {
            
        }

        public void DeclareVariable(string name, Type type)
        {
            var lVar = Generator.DeclareLocal(type);
            _localsByName.Add(name, lVar);
        }

        public void VariableEmitLoadIntoEvalStack(string name)
        {
            if (!_localsByName.TryGetValue(name, out var local))
            {
                // this shouldn't happen at this point.
                throw new Exception($"Variable {name} never declared.");
            }

            if (local.LocalIndex == 0)
            {
                Generator.Emit(OpCodes.Ldloc_0);
            }
            else if (local.LocalIndex == 1)
            {
                Generator.Emit(OpCodes.Ldloc_1);
            }
            else if (local.LocalIndex == 2)
            {
                Generator.Emit(OpCodes.Ldloc_2);
            }
            else if (local.LocalIndex == 3)
            {
                Generator.Emit(OpCodes.Ldloc_3);
            }
            else if (local.LocalIndex <= 255)
            {
                Generator.Emit(OpCodes.Ldloc_S, (byte)local.LocalIndex);
            }
            else
            {
                if (local.LocalIndex > short.MaxValue)
                {
                    throw new Exception($"Too many variables! " +
                        $"The max number of variables that can be declared within a module or lambda is {short.MaxValue}.");
                }

                Generator.Emit(OpCodes.Ldloc, (short)local.LocalIndex);
            }
        }

        public void VariableEmitStoreFromEvalStack(string name)
        {
            if (!_localsByName.TryGetValue(name, out var local))
            {
                // this shouldn't happen if the analyzer is doing its job.
                throw new Exception($"Variable {name} never declared.");
            }

            if (local.LocalIndex == 0)
            {
                Generator.Emit(OpCodes.Stloc_0);
            }
            else if (local.LocalIndex == 1)
            {
                Generator.Emit(OpCodes.Stloc_1);
            }
            else if (local.LocalIndex == 2)
            {
                Generator.Emit(OpCodes.Stloc_2);
            }
            else if (local.LocalIndex == 3)
            {
                Generator.Emit(OpCodes.Stloc_3);
            }
            else if (local.LocalIndex <= 255)
            {
                Generator.Emit(OpCodes.Stloc_S, (byte)local.LocalIndex);
            }
            else
            {
                if (local.LocalIndex > short.MaxValue)
                {
                    throw new Exception($"Too many variables! " +
                        $"The max number of variables that can be declared within a module or lambda is {short.MaxValue}.");
                }

                Generator.Emit(OpCodes.Stloc, (short)local.LocalIndex);
            }
        }
    }
}
