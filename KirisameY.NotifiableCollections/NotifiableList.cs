using System.Collections;
using System.Collections.Immutable;

using KirisameY.Relinq.Extensions;

namespace KirisameY.NotifiableCollections;

public class NotifiableList<T> : INotifiableList<T>
{
    private readonly List<T> _innerList = [];
    private IReadOnlyList<T> Readonly => field ??= _innerList.AsReadOnly();


    #region Reading

    public IEnumerator<T> GetEnumerator() => _innerList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_innerList).GetEnumerator();

    public bool Contains(T item) => _innerList.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

    public int Count => _innerList.Count;

    public bool IsReadOnly => false;

    public int IndexOf(T item) => _innerList.IndexOf(item);

    #endregion


    #region Converting

    // AsReadOnly

    #endregion


    public T this[int index]
    {
        get => _innerList[index];
        set
        {
            var oldValue = _innerList[index];
            _innerList[index] = value;
            ItemReplaced?.Invoke(this, new([oldValue], [value], [index]));
        }
    }

    public void Add(T item)
    {
        _innerList.Add(item);
        ItemAdded?.Invoke(this, new([item], _innerList.Count - 1));
    }

    public void AddRange(ICollection<T> items)
    {
        var fromIndex = _innerList.Count;
        _innerList.AddRange(items);
        ItemAdded?.Invoke(this, new([..items], fromIndex));
    }

    public void AddRange(IEnumerable<T> items) => AddRange([..items]);

    public void Insert(int index, T item)
    {
        _innerList.Insert(index, item);
        ItemAdded?.Invoke(this, new([item], index));
    }

    public void InsertRange(int index, ICollection<T> items)
    {
        _innerList.InsertRange(index, items);
        ItemAdded?.Invoke(this, new([..items], index));
    }

    public void InsertRange(int index, IEnumerable<T> items) => InsertRange(index, [..items]);

    public void Clear()
    {
        var before = _innerList.ToImmutableArray();
        _innerList.Clear();
        ItemRemoved?.Invoke(this, new(before, [..Enumerable.Range(0, before.Length)], true));
    }

    public bool Remove(T item)
    {
        var index = _innerList.IndexOf(item);
        if (index < 0) return false;
        _innerList.RemoveAt(index);
        ItemRemoved?.Invoke(this, new([item], [index], _innerList.Count == 0));
        return true;
    }

    public void RemoveAt(int index)
    {
        var item = _innerList[index];
        _innerList.RemoveAt(index);
        ItemRemoved?.Invoke(this, new([item], [index], _innerList.Count == 0));
    }

    public void RemoveRange(int index, int count)
    {
        var removed = _innerList[index..(index + count)];
        var removedIndexes = Enumerable.Range(index, count).ToImmutableList();
        _innerList.RemoveRange(index, count);
        ItemRemoved?.Invoke(this, new(removed, removedIndexes, _innerList.Count == 0));
    }

    public void RemoveAll(Predicate<T> predicate)
    {
        var indexes = _innerList.Select((item, index) => predicate.Invoke(item) ? index : -1)
                                .Where(i => i >= 0).ToImmutableList();
        var items = indexes.Reverse().Select(i =>
        {
            var item = _innerList[i];
            _innerList.RemoveAt(i);
            return item;
        }).Reverse().ToImmutableList();
        ItemRemoved?.Invoke(this, new(items, indexes, _innerList.Count == 0));
    }

    public void Sort()
    {
        _innerList.Sort();
        Sorted?.Invoke(this, new(Readonly));
    }

    public void Sort(Comparison<T> comparison)
    {
        _innerList.Sort(comparison);
        Sorted?.Invoke(this, new(Readonly));
    }

    public void Sort(IComparer<T> comparer)
    {
        _innerList.Sort(comparer);
        Sorted?.Invoke(this, new(Readonly));
    }

    public void Sort(int index, int count, IComparer<T> comparer)
    {
        _innerList.Sort(index, count, comparer);
        Sorted?.Invoke(this, new(Readonly));
    }

    public void Reverse()
    {
        _innerList.Reverse();
        Sorted?.Invoke(this, new(Readonly));
    }

    public void Reverse(int index, int count)
    {
        _innerList.Reverse(index, count);
        Sorted?.Invoke(this, new(Readonly));
    }


    public event EventHandler<ListItemAddedEventArgs<T>>? ItemAdded;
    public event EventHandler<ListItemRemovedEventArgs<T>>? ItemRemoved;
    public event EventHandler<ListItemReplacedEventArgs<T>>? ItemReplaced;
    public event EventHandler<ListSortedEventArgs<T>>? Sorted;
}