using ClrScript.Elements.Expressions;
using ClrScript.Lexer;
using ClrScript.Visitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements.Statements
{
    class VarStmt : Stmt
    {
        public Token Name { get; }

        public VariableType VariableType { get; }

        public Expr Initializer { get; }

        public override Token StartLocation { get; }

        public VarStmt(Token startLoc, VariableType variableType, Token name, Expr initializer)
        {
            VariableType = variableType;
            StartLocation = startLoc;
            Name = name;
            Initializer = initializer;
        }

        public override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitVarStmt(this);
        }
    }
}
