using ClrScript.Elements.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Analysis
{
    class LambdaAnalysisRecursionDetector
    {
        readonly List<Lambda> _calls = new List<Lambda>();

        public bool RecursionDetected { get; private set; }

        public bool Enter(Lambda call)
        {
            RecursionDetected = _calls.Contains(call);

            if (!RecursionDetected)
            {
                _calls.Add(call);
            }

            return RecursionDetected;
        }

        public void RollbackTo(Lambda call)
        {
            var index = _calls.IndexOf(call);

            for (int i = index; i < _calls.Count; i++)
            {
                _calls.RemoveAt(i);
            }
        }
    }
}
