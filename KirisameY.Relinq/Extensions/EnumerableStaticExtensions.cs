using JetBrains.Annotations;

namespace KirisameY.Relinq.Extensions;

public static class EnumerableStaticExtensions
{
    extension(Enumerable)
    {
        [PublicAPI, Pure]
        public static IEnumerable<T> Flatten<T>(params IEnumerable<IEnumerable<T>> source) => source.Flatten();
    }
}