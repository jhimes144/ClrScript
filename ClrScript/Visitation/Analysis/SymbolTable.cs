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
        public Scope CurrentScope { get; private set; }

        public void BeginScope(ScopeKind kind)
        {
            CurrentScope = new Scope(kind, CurrentScope);
        }

        public void EndScope()
        {
            CurrentScope = CurrentScope.Parent;
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
