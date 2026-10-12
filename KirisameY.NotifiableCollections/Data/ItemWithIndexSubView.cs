using System.Collections;

namespace KirisameY.NotifiableCollections.Data;

internal class ItemWithIndexItemsView<T>(IReadOnlyList<IItemWithIndex<T>> source) : IReadOnlyList<T>
{
    public IEnumerator<T> GetEnumerator() => source.Select(p => p.Item).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Count;
    public T this[int index] => source[index].Item;
}

internal class ItemWithIndexIndexesView<T>(IReadOnlyList<IItemWithIndex<T>> source) : IReadOnlyList<int>
{
    public IEnumerator<int> GetEnumerator() => source.Select(p => p.Index).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Count;
    public int this[int index] => source[index].Index;
}