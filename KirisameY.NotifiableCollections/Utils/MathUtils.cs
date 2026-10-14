using KirisameY.GenericUtils;
using KirisameY.Relinq.Extensions;

namespace KirisameY.NotifiableCollections.Utils;

internal static class MathUtils
{
    public static int[] ResolveLis(IReadOnlyList<int> list)
    {
        var tail = new List<int>(capacity: list.Count);
        var prev = new int[list.Count];
        Array.Fill(prev, -1);

        foreach (var i in list)
        {
            if (tail.Count == 0)
            {
                tail.Add(i);
                continue;
            }

            // 二分查找首个能替换的位置
            var l = BinaryFindFirst(tail, i, static (s, i) => s >= i);

            if (l >= tail.Count) tail.Add(i);
            else tail[l] = i;
            prev[i] = l == 0 ? -1 : tail[l - 1];
        }

        var result = Enumerable.Generate(Optional.From(tail.Last()), i => Optional.From(prev[i], j => j < 0))
                               .Reverse()
                               .ToArray();
        return result;
    }


    public static int BinaryFindFirst<TSource>(IReadOnlyList<TSource> source, Func<TSource, bool> predicate) =>
        BinaryFindFirst(source, predicate, static (source, predict) => predict.Invoke(source));

    public static int BinaryFindFirst<TSource, TParam>(IReadOnlyList<TSource> source, TParam param, Func<TSource, TParam, bool> predicate)
    {
        int l = 0, r = source.Count;
        while (l < r)
        {
            var m = (l + r) / 2;
            if (predicate.Invoke(source[m], param))
                r  = m;
            else l = m + 1;
        }
        return l;
    }

    public static void BinarySortedAdd<T, TList>(TList source, T newItem, Func<T, T, bool> predicate) where TList : IList<T>, IReadOnlyList<T>
    {
        var i = BinaryFindFirst(source, newItem, predicate);
        source.Insert(i, newItem);
    }
}