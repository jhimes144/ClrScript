using ClrScript.Elements;
using ClrScript.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Analysis
{
    enum ScopeKind
    {
        Root,
        Lambda,
        Block,
        Module,
        Blueprint
    }

    class Scope
    {
        readonly Dictionary<string, Symbol> _symbolsByName
            = new Dictionary<string, Symbol>();

        public Scope Parent { get; }

        public ScopeKind Kind { get; set; }

        public Scope(ScopeKind kind, Scope parent)
        {
            Kind = kind;
            Parent = parent;
        }

        public void RegisterSymbol(string name, Symbol symbol)
        {
            if (_symbolsByName.ContainsKey(name))
            {
                throw new Exception("Symbol already registered.");
            }

            _symbolsByName[name] = symbol;
        }

        public void ClearSymbols()
        {
            _symbolsByName.Clear();
        }

        public Symbol FindLocalSymbol(string name)
        {
            return _symbolsByName.GetValueOrDefault(name);
        }

        public Symbol FindSymbolGoingUp(string name, out Scope foundScope)
        {
            var scope = this;

            do
            {
                if (scope._symbolsByName.TryGetValue(name, out var symbol))
                {
                    foundScope = scope;
                    return symbol;
                }

                scope = scope.Parent;
            } while (scope != null);

            foundScope = null;
            return null;
        }
    }
}
