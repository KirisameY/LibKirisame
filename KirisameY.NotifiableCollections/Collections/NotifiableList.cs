using System.Collections;
using System.Collections.Immutable;

using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

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


    #region Writing

    public T this[int index]
    {
        get => _innerList[index];
        set
        {
            var oldValue = _innerList[index];
            _innerList[index] = value;
            RaiseUpdate(new ListItemReplacedEventArgs<T>(Readonly, [oldValue], [value], [index]));
        }
    }

    public void Add(T item)
    {
        _innerList.Add(item);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [item], _innerList.Count - 1));
    }

    public void AddRange(ICollection<T> items)
    {
        var fromIndex = _innerList.Count;
        _innerList.AddRange(items);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [..items], fromIndex));
    }

    public void AddRange(IEnumerable<T> items) => AddRange([..items]);

    public void Insert(int index, T item)
    {
        _innerList.Insert(index, item);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [item], index));
    }

    public void InsertRange(int index, ICollection<T> items)
    {
        _innerList.InsertRange(index, items);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [..items], index));
    }

    public void InsertRange(int index, IEnumerable<T> items) => InsertRange(index, [..items]);

    public void Clear()
    {
        var before = _innerList.ToImmutableArray();
        _innerList.Clear();
        // 索引取 0..旧长度-1：升序连续，且与 before 逐位对应
        RaiseUpdate(new ListItemClearedEventArgs<T>(Readonly, before, [..Enumerable.Range(0, before.Length)]));
    }

    public bool Remove(T item)
    {
        var index = _innerList.IndexOf(item);
        if (index < 0) return false;
        _innerList.RemoveAt(index);
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, [item], [index]));
        return true;
    }

    public void RemoveAt(int index)
    {
        var item = _innerList[index];
        _innerList.RemoveAt(index);
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, [item], [index]));
    }

    public void RemoveRange(int index, int count)
    {
        var removed = _innerList[index..(index + count)];
        // 升序连续，且与 removed 逐位对应
        var removedIndexes = Enumerable.Range(index, count).ToImmutableList();
        _innerList.RemoveRange(index, count);
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, removed, removedIndexes));
    }

    public void RemoveAll(Predicate<T> predicate)
    {
        // 顺着原索引扫一遍再过滤，天然升序
        var indexes = _innerList.Select((item, index) => predicate.Invoke(item) ? index : -1)
                                .Where(i => i >= 0).ToImmutableList();
        // 内层倒序 RemoveAt 是必须的（否则删完前面的索引就错位了），
        // 外层再 Reverse 一次把 items 翻回升序，好跟 indexes 逐位对齐
        var items = indexes.Reverse().Select(i =>
        {
            var item = _innerList[i];
            _innerList.RemoveAt(i);
            return item;
        }).Reverse().ToImmutableList();
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, items, indexes));
    }

    public void Sort()
    {
        _innerList.Sort();
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    public void Sort(Comparison<T> comparison)
    {
        _innerList.Sort(comparison);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    public void Sort(IComparer<T> comparer)
    {
        _innerList.Sort(comparer);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    public void Sort(int index, int count, IComparer<T> comparer)
    {
        _innerList.Sort(index, count, comparer);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    public void Reverse()
    {
        _innerList.Reverse();
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    public void Reverse(int index, int count)
    {
        _innerList.Reverse(index, count);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    #endregion


    private readonly List<EventHandler<ListUpdateEventArgs<T>>> _listUpdatedEventHandlers = [];

    private void RaiseUpdate(ListUpdateEventArgs<T> args)
    {
        foreach (var eventHandler in _listUpdatedEventHandlers)
        {
            eventHandler.Invoke(this, args);
        }
    }

    public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated
    {
        add
        {
            if (value is not null) _listUpdatedEventHandlers.Add(value);
        }
        remove
        {
            if (value is not null) _listUpdatedEventHandlers.Remove(value);
        }
    }
}