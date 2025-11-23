using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace KirisameLib.GeneratorTools.Extensions;

public static class TypeParameterSymbolExtensions
{
    public static string? GetConstraintString(this ITypeParameterSymbol t)
    {
        var nullable = t.ReferenceTypeConstraintNullableAnnotation;

        IEnumerable<string> constraints = [];

        // class / struct / unmanaged
        if (t.HasReferenceTypeConstraint)
        {
            string classConstraint = nullable == NullableAnnotation.Annotated ? "class?" : "class";
            constraints = constraints.Append(classConstraint);
        }
        else if (t.HasValueTypeConstraint)
        {
            constraints = constraints.Append("struct");
        }
        else if (t.HasUnmanagedTypeConstraint)
        {
            constraints = constraints.Append("unmanaged");
        }

        // Base/Interface
        constraints = constraints.Concat(t.ConstraintTypes.Select(s => s.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

        // new
        if (t.HasConstructorConstraint)
        {
            constraints = constraints.Append("new()");
        }

        var constraintsString = constraints.Join(", ");
        return constraintsString is "" ? null : $"where {t.Name}: {constraintsString}";
    }
}