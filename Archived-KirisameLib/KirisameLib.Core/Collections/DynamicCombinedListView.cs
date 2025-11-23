using System.Collections;

using JetBrains.Annotations;

namespace KirisameLib.Collections;

public class DynamicCombinedListView<T>(Func<IEnumerable<IReadOnlyList<T>>> listsGetter) : IReadOnlyList<T>
{
    internal DynamicCombinedListView(IEnumerable<IReadOnlyList<T>> lists) : this(() => lists) { }

    [MustDisposeResource]
    public IEnumerator<T> GetEnumerator() => listsGetter.Invoke().SelectMany(l => l).GetEnumerator();

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => listsGetter.Invoke().Sum(list => list.Count);
    public T this[int index]
    {
        get
        {
            foreach (var list in listsGetter.Invoke())
            {
                if (index >= list.Count) index -= list.Count;
                else return list[index];
            }

            throw new IndexOutOfRangeException();
        }
    }
}