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

        public string ClankName { get; }

        public Type ClrType { get; }

        public ExternalType(string clankName, Type clrType, 
            IReadOnlyList<ExternalTypeMethod> methods,
            IReadOnlyList<ExternalTypeProperty> properties,
            IReadOnlyList<ExternalTypeField> fields)
        {
            ClankName = clankName;
            ClrType = clrType;
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
