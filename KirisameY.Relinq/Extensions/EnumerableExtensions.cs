using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace KirisameY.Relinq.Extensions;

public static class EnumerableExtensions
{
    extension<TSource>(IEnumerable<TSource> source)
    {
        [PublicAPI]
        public void ForEach(Action<TSource> action)
        {
            foreach (var entry in source)
            {
                action.Invoke(entry);
            }
        }

        [PublicAPI]
        public void ForEach(Action<int, TSource> action)
        {
            foreach (var (index, item) in source.Index())
            {
                action.Invoke(index, item);
            }
        }

        [PublicAPI, Pure]
        public IEnumerable<TResult> SelectExist<TResult>(Func<TSource, TResult?> selector) => source.Select(selector).OfType<TResult>();

        [PublicAPI]
        public IEnumerable<TSource> SelectSelf(Action<TSource> selector) => source.Select(e =>
        {
            selector.Invoke(e);
            return e;
        });

        [PublicAPI, Pure]
        public IEnumerable<TResult> SelectWhile<TResult>(WhileSelector<TSource, TResult> selector)
        {
            foreach (var item in source)
            {
                if (!selector.Invoke(item, out var result)) yield break;
                yield return result;
            }
        }

        [PublicAPI, Pure]
        public bool ContainsAny(ICollection<TSource> items) => source.Any(items.Contains); // 应该是反过来但这样更合适

        [PublicAPI, Pure]
        public bool ContainsAll(ICollection<TSource> items)
        {
            if (items.Count == 0) return true;

            var c = items.ToHashSet();
            foreach (var item in source)
            {
                c.Remove(item);
                if (c.Count == 0) return true;
            }

            return false;
        }

        [PublicAPI, Pure]
        public IEnumerable<int> FindAll(TSource find) =>
            source.Index().Where(t => Equals(t.Item, find)).Select(t => t.Index);

        [PublicAPI, Pure]
        public IEnumerable<int> FindAll(Func<TSource, bool> predicate) =>
            source.Index().Where(t => predicate(t.Item)).Select(t => t.Index);

        [PublicAPI, Pure]
        public IEnumerable<(TSource, TSecond)> CrossJoin<TSecond>(IEnumerable<TSecond> second) => source.CrossJoin(second, (x, y) => (x, y));

        [PublicAPI, Pure]
        public IEnumerable<TResult> CrossJoin<TSecond, TResult>(IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> resultSelector) =>
            from x in source
            from y in second
            select resultSelector(x, y);
    }

    extension<TSource>(IEnumerable<IEnumerable<TSource>> source)
    {
        [PublicAPI, Pure]
        public IEnumerable<TSource> Flatten() => source.SelectMany(x => x);
    }

    extension(Enumerable)
    {
        [PublicAPI, Pure]
        public static IEnumerable<T> Flatten<T>(params IEnumerable<T>[] source) => source.Flatten();
    }

    [PublicAPI]
    public delegate bool WhileSelector<in TSource, TResult>(TSource item, [MaybeNullWhen(false)] out TResult result);
}