using Clank.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements
{
    enum TypeCombinationType
    { 
        Union,
        Intersection, 
    }

    class ClankTypeMeta
    {
        public static ClankTypeMeta Number { get; } = new ClankTypeMeta("number");
        public static ClankTypeMeta String { get; } = new ClankTypeMeta("string");
        public static ClankTypeMeta Bool { get; } = new ClankTypeMeta("bool");
        public static ClankTypeMeta Void { get; } = new ClankTypeMeta("void");

        public TypeCombinationType TypeCombinationType { get; }

        public IReadOnlyList<string> Types { get; }

        public string Single => Types.Count == 1 ? Types[0] : null;

        public bool IsSingle => Types.Count == 1;

        public bool IsExternal => ExternalType != null;

        public ExternalType ExternalType { get; }

        public ClankTypeMeta(string type)
        {
            Types = new List<string>()
            {
                type
            };
        }

        public ClankTypeMeta(string type, ExternalType externalType)
        {
            ExternalType = externalType;
            Types = new List<string>()
            {
                type
            };
        }

        public ClankTypeMeta(TypeCombinationType typeCombinationType, IReadOnlyList<string> types)
        {
            if (types.Count == 0)
            {
                throw new InvalidOperationException("Must have one type.");
            }

            TypeCombinationType = typeCombinationType;
            Types = types;
        }

        public override string ToString()
        {
            if (IsSingle)
            {
                return Single;
            }

            return string.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj is ClankTypeMeta other)
            {
                if (other.TypeCombinationType != TypeCombinationType)
                {
                    return false;
                }

                if (other.Types.Count != Types.Count)
                {
                    return false;
                }

                for (var i = 0; i < Types.Count; i++)
                {
                    var thisType = Types[i];
                    var otherType = other.Types[i];

                    if (thisType != otherType)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + TypeCombinationType.GetHashCode();

                for (int i = 0; i < Types.Count; i++)
                {
                    hash = hash * 23 + (Types[i]?.GetHashCode() ?? 0);
                }

                return hash;
            }
        }
    }
}
