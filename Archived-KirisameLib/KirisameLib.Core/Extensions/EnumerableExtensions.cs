using System.Diagnostics.Contracts;

namespace KirisameLib.Extensions;

public static class EnumerableExtensions
{
    //single
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var element in enumerable)
        {
            action(element);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        int index = 0;
        foreach (var element in enumerable)
        {
            action(element, index);
            index++;
        }
    }

    public static int PositionOfFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int order = 0;
        foreach (var item in source)
        {
            if (predicate(item)) return order;
            order++;
        }
        return -1;
    }

    public static int PositionOfFirst<T>(this IEnumerable<T> source, T target) => PositionOfFirst(source, item => Equals(item, target));

    [Pure]
    public static IEnumerable<TResult> SelectExist<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult?> selector) =>
        source.Select(selector).OfType<TResult>();

    public static IEnumerable<T> SelectSelf<T>(this IEnumerable<T> source, Action<T> selector) => source.Select(e =>
    {
        selector(e);
        return e;
    });

    [Pure]
    public static IEnumerable<T> SelectAggregate<T>(this IEnumerable<T> source, Func<T, T, T> aggregator)
    {
        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext()) yield break;
        var working = enumerator.Current;
        yield return working;

        while (enumerator.MoveNext())
        {
            working = aggregator(working, enumerator.Current);
            yield return working;
        }
    }

    [Pure]
    public static IEnumerable<TAccumulate> SelectAggregate<TSource, TAccumulate>(
        this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> aggregator)
    {
        var working = seed;
        foreach (var current in source)
        {
            working = aggregator(working, current);
            yield return working;
        }
    }

    [Pure]
    public static IEnumerable<TResult> SelectAggregate<TSource, TAccumulate, TResult>(
        this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> aggregator,
        Func<TAccumulate, TResult> resultSelector)
    {
        var working = seed;
        foreach (var current in source)
        {
            working = aggregator(working, current);
            yield return resultSelector(working);
        }
    }

    public delegate bool WhileSelector<TSource, TResult>(ref TSource item, out TResult result);

    [Pure]
    public static IEnumerable<TResult> SelectWhile<TSource, TResult>(this TSource source, WhileSelector<TSource, TResult> selector)
    {
        while (selector(ref source, out var result)) yield return result;
    }

    [Pure]
    public static IEnumerable<TResult> SelectWhile<TSource, TResult>(this TSource source, Func<TSource, bool> condition, Func<TSource, TResult> selector)
    {
        while (condition(source))
        {
            yield return selector(source);
        }
    }

    [Pure]
    public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> items) => items.Any(source.Contains);

    [Pure]
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> items) => items.All(source.Contains);

    [Pure]
    public static IEnumerable<int> FindAll<T>(this IEnumerable<T> source, T find) =>
        source.Index().Where(t => Equals(t.Item, find)).Select(t => t.Index);

    [Pure]
    public static IEnumerable<int> FindAll<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Index().Where(t => predicate(t.Item)).Select(t => t.Index);

    //dual
    [Pure]
    public static IEnumerable<(TFirst, TSecond)> CrossJoin<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second) =>
        CrossJoin(first, second, (x, y) => (x, y));

    [Pure]
    public static IEnumerable<TResult> CrossJoin<TFirst, TSecond, TResult>(
        this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) =>
        from x in first
        from y in second
        select resultSelector(x, y);


    //multi
    [Pure]
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) =>
        source.SelectMany(x => x);

    [Pure]
    public static IEnumerable<T> Flatten<T>(params IEnumerable<T>[] source) => source.Flatten();
}