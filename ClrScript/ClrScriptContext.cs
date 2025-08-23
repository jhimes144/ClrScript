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
using ClrScript.Visitation.SymbolCollection;

namespace ClrScript
{
    public interface IClrScriptEntry<TIn>
    {
        object Main(TIn input);
    }

    public class ClrScriptContext<TIn> : IDisposable where TIn : class
    {
        readonly IClrScriptEntry<TIn> _entry;

        internal ClrScriptContext(Type clankType)
        {
            _entry = (IClrScriptEntry<TIn>)Activator.CreateInstance(clankType);
        }

        public object Run()
        {
            return _entry.Main(default);
        }

        public object Run(TIn input)
        {
            return _entry.Main(input);
        }

        public static ClrScriptContext<TIn> Compile(IEnumerable<string> sources, ClrScriptCompilationSettings settings = null)
        {
            var allSource = new StringBuilder();

            foreach (string source in sources)
            {
                allSource.Append(source);
            }

            return Compile(allSource.ToString(), settings);
        }

        public static ClrScriptContext<TIn> Compile(string source, ClrScriptCompilationSettings settings = null)
        {
            settings ??= new ClrScriptCompilationSettings();

            var externalTypeAnalyzer = new ExternalTypeAnalyzer(settings);
            externalTypeAnalyzer.SetInType(typeof(TIn));

            var errors = new List<ClrScriptCompileException>();
            var lexer = new ClrScriptLexer(source);
            var tokens = lexer.Tokenize();

            var parser = new ClrScriptParser(tokens, settings); 
            var parseResult = parser.Parse();

            var symbolTable = new SymbolTable();
            var symbolCollector = new SymbolCollectionVisitor(symbolTable, errors);

            foreach (var statement in parseResult)
            {
                statement.Accept(symbolCollector);
            }

            if (errors.Count > 0)
            {
                errors.Reverse();
                throw new AggregateException(errors);
            }

            var assemblyName = new AssemblyName($"ClrScriptDynamic-{Guid.NewGuid()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var clankModule = assemblyBuilder.DefineDynamicModule("ClrScriptModule");
            var defaultClrScript = clankModule.DefineType("Default", TypeAttributes.Public);
            defaultClrScript.AddInterfaceImplementation(typeof(IClrScriptEntry<TIn>));

            var compileContext = new CompilationContext(settings,
                symbolTable,
                externalTypeAnalyzer,
                defaultClrScript);

            foreach (var statement in parseResult)
            {
                statement.Accept(compileContext.StatementCompiler);
            }

            if (!compileContext.ReturnPrepped)
            {
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ldnull);
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ret);
            }

            var clankType = defaultClrScript.CreateType();
            return new ClrScriptContext<TIn>(clankType);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
