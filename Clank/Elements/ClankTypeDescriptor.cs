using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Elements
{
    enum TypeCombinationType
    {
        None,
        Union,
        Intersection,
    }

    class ClankTypeDescriptor
    {
        public TypeCombinationType TypeCombinationType { get; }

        public IReadOnlyList<ClankType> Types { get; }

        public ClankType Single => Types.Count == 1 ? Types[0] : null;

        public bool IsSingle => Types.Count == 1;

        public ClankTypeDescriptor(TypeCombinationType typeCombinationType, IReadOnlyList<ClankType> types)
        {
            if (types.Count == 0)
            {
                throw new InvalidOperationException("Must have one type.");
            }

            TypeCombinationType = typeCombinationType;
            Types = types;
        }

        public ClankTypeDescriptor(ClankType singleType)
        {
            Types = new List<ClankType>()
            {
                singleType,
            };
        }

        public static ClankTypeDescriptor CreateFromString(string str)
        {
            var combType = TypeCombinationType.None;
            var strBuilder = new StringBuilder();

            var descriptorTypeNames = new HashSet<string>();

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (char.IsLetterOrDigit(c))
                {
                    strBuilder.Append(c);
                    continue;
                }

                if (c == '|')
                {
                    if (combType == TypeCombinationType.Intersection)
                    {
                        throw new Exception("Combination of type descriptor union and intersection is not supported.");
                    }

                    if (descriptorTypeNames.Add(strBuilder.ToString()))
                    {
                        combType = TypeCombinationType.Union;
                        strBuilder.Clear();
                        continue;
                    }
                    else
                    {
                        throw new Exception("Duplicate type identifier in type descriptor.");
                    }
                }

                if (c == '&')
                {
                    if (combType == TypeCombinationType.Union)
                    {
                        throw new Exception("Combination of type descriptor union and intersection is not supported.");
                    }

                    if (descriptorTypeNames.Add(strBuilder.ToString()))
                    {
                        combType = TypeCombinationType.Intersection;
                        strBuilder.Clear();
                        continue;
                    }
                    else
                    {
                        throw new Exception("Duplicate type identifier in type descriptor.");
                    }
                }

                throw new Exception($"Invalid character '{c}' in type descriptor.");
            }

            var types = descriptorTypeNames
                .Select(n => new ClankType(n))
                .ToArray();

            return new ClankTypeDescriptor(combType, types);
        }

        public override string ToString()
        {
            return string.Join(TypeCombinationType == TypeCombinationType.Union ? "|" : "&", Types);
        }

        public override bool Equals(object obj)
        {
            if (obj is ClankTypeDescriptor other)
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
