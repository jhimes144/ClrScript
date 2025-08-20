using Clank.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation.TypeBuilding
{
    class RuntimeTypeContext
    {
        readonly ExternalTypeAnalyzer _externalTypeAnalyzer;

        public RuntimeTypeContext(ExternalTypeAnalyzer externalTypeAnalyzer)
        {
            _externalTypeAnalyzer = externalTypeAnalyzer;
        }

        
    }

}
