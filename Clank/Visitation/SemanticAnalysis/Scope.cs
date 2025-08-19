using Clank.Elements.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.SemanticAnalysis
{
    class Scope
    {
        readonly Dictionary<string, VarStmt> _variablesByName 
            = new Dictionary<string, VarStmt>();

        readonly AnalysisContext _context;

        public Scope Parent { get; }

        public Scope(AnalysisContext context, Scope parent)
        {
            _context = context;
            Parent = parent;
        }

        public bool TryRegisterVariableDeclaration(VarStmt varStmt)
        {
            var scope = this;

            do
            {
                if (scope._variablesByName.ContainsKey(varStmt.Name.Value))
                {
                    if (scope == this)
                    {
                        _context.Errors.Add(new ClankCompileException($"Variable '{varStmt.Name.Value}'" +
                            $" has already been defined in this scope.", varStmt.Name));
                    }
                    else
                    {
                        _context.Errors.Add(new ClankCompileException($"'{varStmt.Name.Value}'" +
                            $" has already been defined in a parent scope as a variable or parameter.", varStmt.Name));
                    }

                    return false;
                }

                scope = scope.Parent;
            } while (scope != null);

            _variablesByName.Add(varStmt.Name.Value, varStmt);
            return true;
        }

        public VarStmt GetVarDeclaration(string name)
        {
            var scope = this;

            do
            {
                if (scope._variablesByName.TryGetValue(name, out VarStmt stmt))
                {
                    return stmt;
                }

                scope = Parent;
            } while (scope != null);

            return null;
        }
    }
}
