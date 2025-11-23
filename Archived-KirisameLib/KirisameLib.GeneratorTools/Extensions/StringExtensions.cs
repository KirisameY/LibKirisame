using System.Collections.Generic;

namespace KirisameLib.GeneratorTools.Extensions;

public static class StringExtensions
{
    public static string Join(this IEnumerable<string> parts, string separator) => string.Join(separator, parts);
}