using ClrScript.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Elements
{
    abstract class Element
    {
        public abstract Token StartLocation { get; }
    }
}
