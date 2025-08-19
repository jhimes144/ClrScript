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
using Clank.Visitation.SemanticAnalysis;

namespace Clank
{
    public interface IClankEntry<TIn, TOut>
    {
        TOut Main(TIn input);
    }

    public class ClankContext<TIn, TOut> : IDisposable
    {
        readonly IClankEntry<TIn, TOut> _entry;

        internal ClankContext(Type clankType)
        {
            _entry = (IClankEntry<TIn, TOut>)Activator.CreateInstance(clankType);
        }

        public TOut Run()
        {
            return _entry.Main(default);
        }

        public TOut Run(TIn input)
        {
            return _entry.Main(input);
        }

        public static ClankContext<TIn, TOut> Compile(IEnumerable<string> sources, ClankCompilationSettings settings = null)
        {
            var allSource = new StringBuilder();

            foreach (string source in sources)
            {
                allSource.Append(source);
            }

            return Compile(allSource.ToString(), settings);
        }

        public static ClankContext<TIn, TOut> Compile(string source, ClankCompilationSettings settings = null)
        {
            verifyThrowAllowedInteropType(typeof(TIn));
            verifyThrowAllowedInteropType(typeof(TOut));

            settings ??= new ClankCompilationSettings();

            var lexer = new ClankLexer(source);
            var tokens = lexer.Tokenize();

            var parser = new ClankParser(tokens, settings); 
            var parseResult = parser.Parse();

            var analyzerContext = new AnalysisContext(settings);

            foreach (var statement in parseResult)
            {
                statement.Accept(analyzerContext.StatementAnalyzer);
            }

            if (analyzerContext.Errors.Count > 0)
            {
                analyzerContext.Errors.Reverse();
                throw new AggregateException(analyzerContext.Errors);
            }

            var contextId = Guid.NewGuid();
            var assemblyName = new AssemblyName($"Clank-{contextId}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("ClankModule");

            // not to be confused with a clank module, this is different.
            var defaultClank = moduleBuilder.DefineType("Default", TypeAttributes.Public);
            defaultClank.AddInterfaceImplementation(typeof(IClankEntry<TIn, TOut>));

            var compileContext = new CompilationContext(settings, defaultClank, typeof(TIn), typeof(TOut));

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
            return new ClankContext<TIn, TOut>(clankType);
        }

        static void verifyThrowAllowedInteropType(Type type)
        {
            // todo: does not handle recursive properties.

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
