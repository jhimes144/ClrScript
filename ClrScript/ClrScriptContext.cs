using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClrScript.Lexer;
using ClrScript.Parser;
using ClrScript.Visitation.Compilation;
using ClrScript.Elements.Statements;
using ClrScript.Interop;
using ClrScript.Visitation;
using Microsoft.VisualBasic;
using ClrScript.TypeManagement;

namespace ClrScript
{
    public interface IClrScriptEntry<TIn>
    {
        object Main(TIn input, TypeManager typeManager);
    }

    public class ClrScriptContext<TIn> where TIn : class
    {
        readonly IClrScriptEntry<TIn> _entry;
        readonly TypeManager _typeManager;

        public ClrScriptContext(ClrScriptCompilation<TIn> compilation)
        {
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            _typeManager = compilation.TypeManager;
            _entry = (IClrScriptEntry<TIn>)Activator.CreateInstance(compilation.BuiltRootType);
        }

        public static ClrScriptContext<TIn> Compile(string source, ClrScriptCompilationSettings settings = null)
        {
            var ir = ClrScriptIR<TIn>.Build(source, settings);
            var compilation = ClrScriptCompilation<TIn>.Compile(ir);
            return new ClrScriptContext<TIn>(compilation);
        }

        public object Run()
        {
            return Run(default);
        }

        public object Run(TIn input)
        {
            return _entry.Main(input, _typeManager);
        }
    }
}
