using JetBrains.Annotations;

namespace KirisameY.Relinq.Extensions;

public static class EnumerableStaticExtensions
{
    extension(Enumerable)
    {
        [PublicAPI, Pure]
        public static IEnumerable<T> Flatten<T>(params IEnumerable<IEnumerable<T>> source) => source.Flatten();

        [PublicAPI, Pure]
        public static IEnumerable<T> Generate<T>(T? seed, Func<T, T?> next) where T : notnull
        {
            var item = seed;
            while (item is not null)
            {
                yield return item;
                item = next.Invoke(item);
            }
        }
    }
}