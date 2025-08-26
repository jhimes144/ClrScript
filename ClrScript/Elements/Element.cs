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
        public Type InferredType { get; set; }

        /// <summary>
        /// Simply returns the inferred type if it exists, or system.object if type could not be inferred.
        /// </summary>
        /// <returns></returns>
        public Type GetInferredType() => InferredType ?? typeof(object);

        public abstract Token StartLocation { get; }
    }
}
