using Clank.Elements;
using Clank.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.SemanticAnalysis
{
    class SymbolTable
    {
        readonly AnalysisContext _context;
        readonly Dictionary<Element, Symbol> _symbolsByElement 
            = new Dictionary<Element, Symbol>();

        public Scope CurrentScope { get; private set; }

        public SymbolTable(AnalysisContext context)
        {
            _context = context;

            CurrentScope = new Scope(_context, null);
        }

        public void BeginScope()
        {
            CurrentScope = new Scope(_context, CurrentScope);
        }

        public void EndScope()
        {
            CurrentScope = CurrentScope.Parent;
        }

        public Symbol GetSymbolFor(Element element)
        {
            if (_symbolsByElement.TryGetValue(element, out Symbol symbol))
            {
                return symbol;
            }

            return new Symbol();
        }

        public void SetType(Element element, Type type)
        {
            if (_symbolsByElement.TryGetValue(element, out Symbol symbol))
            {
                symbol.Type = type;
            }
            else
            {
                symbol = new Symbol();
                symbol.Type = type;
                _symbolsByElement[element] = symbol;
            }
        }
    }

    class Symbol
    {
        public Type Type { get; set; }
    }
}
