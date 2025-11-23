using System;

using Microsoft.CodeAnalysis;

namespace KirisameLib.GeneratorTools.Extensions;

public static class AccessibilityExtensions
{
    public static string ToDefinitionString(this Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public               => "public ",
        Accessibility.ProtectedOrInternal  => "protected internal ",
        Accessibility.Internal             => "internal ",
        Accessibility.Protected            => "protected ",
        Accessibility.ProtectedAndInternal => "private protected ",
        Accessibility.Private              => "private ",
        Accessibility.NotApplicable        => "",
        _                                  => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, "Accessibility out of range")
    };
}