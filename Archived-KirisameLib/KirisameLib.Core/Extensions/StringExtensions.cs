namespace KirisameLib.Extensions;

public static class StringExtensions
{
    public static string Join(this IEnumerable<string> parts, string separator) => string.Join(separator, parts);
    public static string Join(this IEnumerable<string> parts, char separator) => string.Join(separator, parts);
}