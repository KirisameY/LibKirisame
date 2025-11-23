using System;

using Microsoft.CodeAnalysis;

namespace KirisameLib.GeneratorTools.Extensions;

public static class ParameterSymbolExtensions
{
    public static string ToDefinitionString(this IParameterSymbol p)
    {
        var @ref = p.RefKind.ToDefinitionString();
        var @params = p.IsParams ? "params " : "";

        var nullable = p.NullableAnnotation == NullableAnnotation.Annotated ? "?" : "";
        return $"{@ref}{@params}{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}{nullable} @{p.Name}";
    }

    public static string ToDefinitionString(this RefKind r) => r switch
    {
        RefKind.None                 => "",
        RefKind.In                   => "in ",
        RefKind.Out                  => "out ",
        RefKind.Ref                  => "ref ",
        RefKind.RefReadOnlyParameter => "ref readonly ",
        _                            => throw new ArgumentOutOfRangeException(nameof(r), r, "RefKind out of range")
    };
}