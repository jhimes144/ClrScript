using ClrScript.Elements;
using ClrScript.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClrScript.Visitation.Analysis
{
    class SymbolTable
    {
        readonly Dictionary<Element, Scope> _scopesByElement 
            = new Dictionary<Element, Scope>();

        Scope _rootScope;

        public Scope CurrentScope { get; private set; }

        public void BeginScope(Element source)
        {
            if (_scopesByElement.TryGetValue(source, out var scope))
            {
                CurrentScope = scope;
            }
            else
            {
                throw new Exception("No scope declared for element.");
            }
        }

        public void BeginRootScope()
        {
            CurrentScope = _rootScope;
        }

        public void DeclareRootScope()
        {
            if (_rootScope != null)
            {
                throw new Exception("Cannot start root scope. Scope already started.");
            }

            CurrentScope = new Scope(ScopeKind.Root, null);
            _rootScope = CurrentScope;
        }

        public void DeclareScope(Element source, ScopeKind kind)
        {
            CurrentScope = new Scope(kind, CurrentScope);
            _scopesByElement[source] = CurrentScope;
        }

        public void EndScope()
        {
            CurrentScope = CurrentScope.Parent;
        }

        public void DestroyChildren(Scope targetScope)
        {
            var elementsToRemove = new List<Element>();

            foreach (var kvp in _scopesByElement)
            {
                var element = kvp.Key;
                var scope = kvp.Value;

                if (IsDescendantOf(scope, targetScope))
                {
                    elementsToRemove.Add(element);
                }
            }

            foreach (var element in elementsToRemove)
            {
                _scopesByElement.Remove(element);
            }
        }

        private bool IsDescendantOf(Scope scope, Scope targetScope)
        {
            if (scope == null || targetScope == null)
            {
                return false;
            }

            var current = scope.Parent;

            while (current != null)
            {
                if (current == targetScope)
                {
                    return true;
                }

                current = current.Parent;
            }

            return false;
        }
    }

    abstract class Symbol
    {
        public Element Element { get; }

        public Scope DeclaringScope { get; }

        public string Name { get; }

        public Symbol(string name, Element element, Scope declaringScope)
        {
            Element = element;
            Name = name;
            DeclaringScope = declaringScope;
            declaringScope.RegisterSymbol(name, this);
        }
    }

    class VariableSymbol : Symbol
    {
        public VariableType VariableType { get; set; }

        public VariableSymbol(string name, Element element, Scope declaringScope) 
            : base(name, element, declaringScope)
        {
        }
    }

    class LambdaParamSymbol : Symbol
    {
        public int ParamIndex { get; }

        public LambdaParamSymbol(int paramIndex, string name, Element element, Scope declaringScope)
            : base(name, element, declaringScope)
        {
            ParamIndex = paramIndex;
        }
    }
}
