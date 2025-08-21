using Clank.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements
{
    class ClankType
    {
        public static ClankType Number { get; } = new ClankType("number");
        public static ClankType String { get; } = new ClankType("string");
        public static ClankType Bool { get; } = new ClankType("bool");
        public static ClankType Void { get; } = new ClankType("void");

        public bool IsExternal => ExternalType != null;

        public ExternalType ExternalType { get; }

        public string TypeName { get; }

        public List<ClankProperty> Properties { get; } = new List<ClankProperty>();

        public ClankType(string typeName)
        {
            TypeName = typeName;
        }

        public ClankType(string typeName, ExternalType externalType)
        {
            ExternalType = externalType;
            TypeName = typeName;
        }

        public override string ToString()
        {
            return TypeName;
        }

        public override bool Equals(object obj)
        {
            if (obj is ClankType other)
            {
                return other.TypeName == TypeName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }
    }

    class ClankProperty
    {
        public string Name { get; }

        public ClankType Type { get; }

        public ClankProperty(string name, ClankType type)
        {
            Name = name;
            Type = type;
        }
    }
}
