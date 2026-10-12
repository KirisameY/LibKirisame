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

        [PublicAPI, Pure]
        public IEnumerable<TResult> Zip<TSecond, TThird, TResult>(
            IEnumerable<TSecond> second, IEnumerable<TThird> third,
            Func<TSource, TSecond, TThird, TResult> resultSelector
        ) => source.Zip(second, third)
                   .Select(t => resultSelector.Invoke(t.First, t.Second, t.Third));

        [PublicAPI, Pure]
        public IEnumerable<TResult> ZipChecked<TSecond, TResult>(IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> resultSelector)
        {
            using var sourceEnumerator = source.GetEnumerator();
            using var secondEnumerator = second.GetEnumerator();
            while (true)
            {
                switch (sourceEnumerator.MoveNext(), secondEnumerator.MoveNext())
                {
                    case (true, true):   yield return resultSelector.Invoke(sourceEnumerator.Current, secondEnumerator.Current); break;
                    case (false, false): yield break;
                    default:             throw new ArgumentException("Zipping collections has mismatched length.");
                }
            }
        }

        [PublicAPI, Pure]
        public IEnumerable<(TSource, TSecond)> ZipChecked<TSecond>(IEnumerable<TSecond> second) =>
            source.ZipChecked(second, (f, s) => (f, s));

        [PublicAPI, Pure]
        public IEnumerable<TResult> ZipChecked<TSecond, TThird, TResult>(
            IEnumerable<TSecond> second, IEnumerable<TThird> third,
            Func<TSource, TSecond, TThird, TResult> resultSelector
        )
        {
            using var sourceEnumerator = source.GetEnumerator();
            using var secondEnumerator = second.GetEnumerator();
            using var thirdEnumerator = third.GetEnumerator();
            while (true)
            {
                switch (sourceEnumerator.MoveNext(), secondEnumerator.MoveNext(), thirdEnumerator.MoveNext())
                {
                    case (true, true, true):    yield return resultSelector.Invoke(sourceEnumerator.Current, secondEnumerator.Current, thirdEnumerator.Current); break;
                    case (false, false, false): yield break;
                    default:                    throw new ArgumentException("Zipping collections has mismatched length.");
                }
            }
        }

        [PublicAPI, Pure]
        public IEnumerable<(TSource, TSecond, TThird)> ZipChecked<TSecond, TThird>(
            IEnumerable<TSecond> second, IEnumerable<TThird> third
        ) => source.ZipChecked(second, third, (f, s, t) => (f, s, t));
    }

    extension<TSource>(IEnumerable<IEnumerable<TSource>> source)
    {
        [PublicAPI, Pure]
        public IEnumerable<TSource> Flatten() => source.SelectMany(x => x);
    }
}