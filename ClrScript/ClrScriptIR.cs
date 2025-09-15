using ClrScript.Elements.Statements;
using ClrScript.Interop;
using ClrScript.Lexer;
using ClrScript.Parser;
using ClrScript.Runtime;
using ClrScript.TypeManagement;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript
{
    /// <summary>
    /// Represents CLRScript right before its ready to be compiled.
    /// You can use this directly to provide errors and other feedback to your
    /// users on code without performing the actual compilation.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public class ClrScriptIR<TIn>
    {
        public IReadOnlyList<ClrScriptCompileError> Errors { get; }
        public IReadOnlyList<ClrScriptCompileError> Warnings { get; }

        internal IReadOnlyList<Stmt> Statements { get; }
        internal SymbolTable SymbolTable { get; }
        internal ShapeTable ShapeTable { get; }
        internal ClrScriptCompilationSettings Settings { get; }
        internal TypeManager TypeManager { get; }
        internal Type InType { get; }

        internal ClrScriptIR(IReadOnlyList<ClrScriptCompileError> errors,
             IReadOnlyList<Stmt> statements,
            IReadOnlyList<ClrScriptCompileError> warnings,
            SymbolTable symbolTable,
            ShapeTable shapeTable,
            ClrScriptCompilationSettings settings,
            TypeManager typeManager,
            Type inType)
        {
            Errors = errors;
            Warnings = warnings;
            Statements = statements;
            SymbolTable = symbolTable;
            ShapeTable = shapeTable;
            Settings = settings;
            TypeManager = typeManager;
            InType = inType;
        }

        public static ClrScriptIR<TIn> Build(IEnumerable<string> sources, ClrScriptCompilationSettings settings = null)
        {
            var allSource = new StringBuilder();

            foreach (string source in sources)
            {
                allSource.Append(source);
            }

            return Build(allSource.ToString(), settings);
        }

        public static ClrScriptIR<TIn> Build(string source, ClrScriptCompilationSettings settings = null)
        {
            settings ??= new ClrScriptCompilationSettings();

            var typeManager = new TypeManager();
            typeManager.ValidatePrepareType(typeof(StringOperations), true);

            if (settings.ExtensionTypes != null)
            {
                foreach (var type in settings.ExtensionTypes)
                {
                    typeManager.ValidatePrepareType(type, true);
                }
            }

            var inType = typeof(TIn);
            var allErrors = new List<ClrScriptCompileError>();
            var lexer = new ClrScriptLexer(source);
            var tokens = lexer.Tokenize();

            var parser = new ClrScriptParser(tokens, settings);
            var parseResult = parser.Parse();

            var symbolTable = new SymbolTable();
            var shapeTable = new ShapeTable(inType);
            var analyzer = new AnalyzerVisitor(symbolTable, typeManager, shapeTable, inType, allErrors);

            foreach (var statement in parseResult)
            {
                statement.Accept(analyzer);
            }

            var errors = allErrors.Where(e => !e.IsWarning).Reverse().ToArray();
            var warnings = allErrors.Where(e => e.IsWarning).Reverse().ToArray();

            return new ClrScriptIR<TIn>(errors, parseResult, warnings, symbolTable, shapeTable,
                settings, typeManager, inType);
        }
    }
}
