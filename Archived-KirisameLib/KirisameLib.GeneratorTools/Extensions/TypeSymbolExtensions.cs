using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace KirisameLib.GeneratorTools.Extensions;

public static class TypeSymbolExtensions
{
    public static bool IsDerivedFrom(this ITypeSymbol? type, ITypeSymbol baseType)
    {
        while (type != null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType)) return true;
            type = type.BaseType;
        }
        return false;
    }

    public static bool IsDerivedFrom(this ITypeSymbol? type, string baseType)
    {
        while (type != null)
        {
            if (type.ToDisplayString() == baseType) return true;
            type = type.BaseType;
        }
        return false;
    }

    public static IEnumerable<AttributeData> GetAllAttributes(this ITypeSymbol type)
    {
        foreach (var attribute in type.GetAttributes())
        {
            yield return attribute;
        }

        var baseType = type.BaseType;
        while (baseType != null)
        {
            foreach (var attribute in baseType.GetAttributes())
            {
                var usage = attribute
                           .AttributeClass!
                           .GetAttributes()
                           .FirstOrDefault(att => att.AttributeClass!.ToDisplayString() == "System.AttributeUsageAttribute");
                var inherited = usage?.NamedArguments.FirstOrDefault(arg => arg.Key == "Inherited").Value.Value;
                if (inherited is false) continue; //not inheritable attribute

                yield return attribute;
            }
            baseType = baseType.BaseType;
        }
    }
}