using JetBrains.Annotations;

namespace KirisameY.Relinq.Extensions;

public static class StringEnumerableExtensions
{
    extension(IEnumerable<string> source)
    {
        [PublicAPI, Pure]
        public string Join(string separator = "") => string.Join(separator, source);

        [PublicAPI, Pure]
        public string Join(char separator) => string.Join(separator, source);
    }
}