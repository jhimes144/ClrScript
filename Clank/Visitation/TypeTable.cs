using Clank.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clank.Visitation
{
    class TypeTable
    {
        readonly SymbolTable _symbols;

        public TypeTable(SymbolTable symbols)
        {
            _symbols = symbols;
        }

        public void GenerateTypesFromSymbols(List<ClankCompileException> errors)
        {
            var symbols = _symbols.SymbolsByElement.Values;
            var types = new HashSet<ClankType>();

            var blueprints = symbols
                .Where(s => s is BlueprintSymbol bp)
                .ToArray();

            var allBlueprintProps = symbols
                .Where(s => s is BlueprintPropSymbol bp)
                .Cast<BlueprintPropSymbol>()
                .ToArray();

            foreach (var blueprintSym in blueprints)
            {
                var type = new ClankType(blueprintSym.Name);
                blueprintSym.ClankType = new ClankTypeDescriptor(type);
                types.Add(type);
            }

            // todo: external types

            foreach (var blueprintPropSym in allBlueprintProps)
            {
                if (!tryMakeDescriptor(blueprintPropSym.PropTypeName, blueprintPropSym.Element,
                    types, errors, out var typeDes))
                {
                    continue;
                }

                blueprintPropSym.ClankType = typeDes;
            }
        }

        bool tryMakeDescriptor(string typeDescriptor, Element errorReporter, HashSet<ClankType> types,
            List<ClankCompileException> errors, out ClankTypeDescriptor descriptor)
        {
            descriptor = null;

            try
            {
                descriptor = ClankTypeDescriptor.CreateFromString(typeDescriptor);
            }
            catch (Exception e)
            {
                errors.Add(new ClankCompileException(e.Message, errorReporter));
            }

            if (descriptor == null)
            {
                return false;
            }

            foreach (var type in descriptor.Types)
            {
                if (!types.Contains(type))
                {
                    errors.Add(new ClankCompileException($"The type or blueprint '{type}' could not be found.",
                        errorReporter));

                    return false;
                }
            }

            return true;
        }
    }
}
