using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Interop
{
    class ExternalType
    {
        public string Error { get; }

        public IReadOnlyList<ExternalTypeMethod> Methods { get; }

        public IReadOnlyList<ExternalTypeProperty> Properties { get; }

        public IReadOnlyList<ExternalTypeField> Fields { get; }

        public ExternalType(IReadOnlyList<ExternalTypeMethod> methods,
            IReadOnlyList<ExternalTypeProperty> properties,
            IReadOnlyList<ExternalTypeField> fields)
        {
            Methods = methods;
            Properties = properties;
            Fields = fields;
        }

        public ExternalType(string error)
        {
            Error = error;
        }
    }
}
