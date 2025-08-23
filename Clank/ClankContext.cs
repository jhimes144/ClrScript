using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Clank.Lexer;
using Clank.Parser;
using Clank.Visitation.Compilation;
using Clank.Elements.Statements;
using Clank.Interop;
using Clank.Visitation;
using Clank.Visitation.SymbolCollection;

namespace Clank
{
    public interface IClankEntry<TIn>
    {
        object Main(TIn input);
    }

    public class ClankContext<TIn> : IDisposable where TIn : class
    {
        readonly IClankEntry<TIn> _entry;

        internal ClankContext(Type clankType)
        {
            _entry = (IClankEntry<TIn>)Activator.CreateInstance(clankType);
        }

        public object Run()
        {
            return _entry.Main(default);
        }

        public object Run(TIn input)
        {
            return _entry.Main(input);
        }

        public static ClankContext<TIn> Compile(IEnumerable<string> sources, ClankCompilationSettings settings = null)
        {
            var allSource = new StringBuilder();

            foreach (string source in sources)
            {
                allSource.Append(source);
            }

            return Compile(allSource.ToString(), settings);
        }

        public static ClankContext<TIn> Compile(string source, ClankCompilationSettings settings = null)
        {
            settings ??= new ClankCompilationSettings();

            var externalTypeAnalyzer = new ExternalTypeAnalyzer(settings);
            externalTypeAnalyzer.SetInType(typeof(TIn));

            var errors = new List<ClankCompileException>();
            var lexer = new ClankLexer(source);
            var tokens = lexer.Tokenize();

            var parser = new ClankParser(tokens, settings); 
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

            var assemblyName = new AssemblyName($"ClankDynamic-{Guid.NewGuid()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var clankModule = assemblyBuilder.DefineDynamicModule("ClankModule");
            var defaultClank = clankModule.DefineType("Default", TypeAttributes.Public);
            defaultClank.AddInterfaceImplementation(typeof(IClankEntry<TIn>));

            var compileContext = new CompilationContext(settings,
                symbolTable,
                externalTypeAnalyzer,
                defaultClank);

            foreach (var statement in parseResult)
            {
                statement.Accept(compileContext.StatementCompiler);
            }

            if (!compileContext.ReturnPrepped)
            {
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ldnull);
                compileContext.CurrentEnv.Generator.Emit(OpCodes.Ret);
            }

            var clankType = defaultClank.CreateType();
            return new ClankContext<TIn>(clankType);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
