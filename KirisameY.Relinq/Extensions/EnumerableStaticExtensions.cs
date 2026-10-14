using JetBrains.Annotations;

using KirisameY.GenericUtils;

namespace KirisameY.Relinq.Extensions;

public static class EnumerableStaticExtensions
{
    extension(Enumerable)
    {
        [PublicAPI, Pure]
        public static IEnumerable<T> Flatten<T>(params IEnumerable<IEnumerable<T>> source) => source.Flatten();

        [PublicAPI, Pure]
        public static IEnumerable<T> Generate<T>(Optional<T> seed, Func<T, Optional<T>> next)
        {
            var item = seed;
            while (item.HasValue)
            {
                yield return item.Value;
                item = next.Invoke(item.Value);
            }
        }
    }
}