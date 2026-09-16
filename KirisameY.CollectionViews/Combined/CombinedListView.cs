using System.Collections;
using System.Collections.Immutable;

using KirisameY.Relinq.Extensions;

namespace KirisameY.CollectionViews.Combined;

public class CombinedListView<T>(IReadOnlyCollection<IReadOnlyList<T>> lists) : IReadOnlyList<T>
{
    public static CombinedListView<T> Create(IReadOnlyCollection<IReadOnlyList<T>> lists) => new(lists);
    public static CombinedListView<T> CreateFrozen(IEnumerable<IReadOnlyList<T>> lists) => new([..lists]);


    public IEnumerator<T> GetEnumerator() => lists.Flatten().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => lists.Sum(l => l.Count);
    public T this[int index]
    {
        get
        {
            var i = index;
            foreach (var list in lists)
            {
                if (i < list.Count) return list[i];
                i -= list.Count;
            }

            throw new IndexOutOfRangeException($"Index is {index} but {nameof(CombinedListView<>)} length is {Count}");
        }
    }
}