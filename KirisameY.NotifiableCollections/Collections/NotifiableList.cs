using System.Collections;
using System.Collections.Immutable;

using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可通知的列表：在 <see cref="IList{T}"/> 的基础上，把每次改动作为 <see cref="ListUpdated"/> 事件发出去。
///     <br/>
///     A notifiable list: an <see cref="IList{T}"/> that publishes every change through the
///     <see cref="ListUpdated"/> event.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <remarks>
///     <para>
///         与 <c>ObservableCollection&lt;T&gt;</c> 只报告"发生了什么"不同，这里的事件参数连变更项带它们的原始索引
///         一并给出（见 <see cref="IListUpdateEventArgs{T}"/> 一族），订阅方不必回头再读一遍列表就能定位变化。
///     </para>
///     <para>
///         通知是在内部列表<b>改完之后</b>同步发出的：事件参数里的视图是活视图而非快照，
///         而各类 <c>Indexes</c> 记的仍是变更<b>前</b>的原始索引。
///         另外，排序与反转发出的是重排通知，不携带任何索引信息，需要新顺序请直接读视图。
///     </para>
///     <para>
///         Unlike <c>ObservableCollection&lt;T&gt;</c>, which merely reports that something happened, the event
///         arguments here carry the changed items together with their original indices (see the
///         <see cref="IListUpdateEventArgs{T}"/> family), so subscribers can locate a change without re-reading
///         the list.
///     </para>
///     <para>
///         Notifications are raised synchronously <b>after</b> the underlying list has been modified: the view
///         carried by the event arguments is a live view rather than a snapshot, while the various <c>Indexes</c>
///         still record the original indices from <b>before</b> the change. Sorting and reversing raise reorder
///         notifications, which carry no index information at all — read the view directly for the new order.
///     </para>
/// </remarks>
public class NotifiableList<T> : INotifiableList<T>
{
    private readonly List<T> _innerList = [];
    private IReadOnlyList<T> Readonly => field ??= _innerList.AsReadOnly();


    #region Reading

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => _innerList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_innerList).GetEnumerator();

    /// <inheritdoc/>
    public bool Contains(T item) => _innerList.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

    /// <inheritdoc cref="IReadOnlyCollection{T}.Count" />
    public int Count => _innerList.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public int IndexOf(T item) => _innerList.IndexOf(item);

    #endregion


    #region Writing

    /// <inheritdoc cref="INotifiableList{T}.this" />
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

    /// <inheritdoc/>
    public void Add(T item)
    {
        _innerList.Add(item);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [item], _innerList.Count - 1));
    }

    /// <summary>
    ///     在末尾追加一批元素，并发出<b>一次</b>携带整批元素的添加通知。
    ///     <br/>
    ///     Appends a batch of elements and raises a <b>single</b> addition notification carrying the whole batch.
    /// </summary>
    public void AddRange(ICollection<T> items)
    {
        var fromIndex = _innerList.Count;
        _innerList.AddRange(items);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [..items], fromIndex));
    }

    /// <inheritdoc cref="AddRange(ICollection{T})"/>
    public void AddRange(IEnumerable<T> items) => AddRange([..items]);

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        _innerList.Insert(index, item);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [item], index));
    }

    /// <summary>
    ///     从指定位置起插入一批元素，并发出<b>一次</b>携带整批元素的添加通知。
    ///     <br/>
    ///     Inserts a batch of elements at the given position and raises a <b>single</b> addition notification
    ///     carrying the whole batch.
    /// </summary>
    public void InsertRange(int index, ICollection<T> items)
    {
        _innerList.InsertRange(index, items);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [..items], index));
    }

    /// <inheritdoc cref="InsertRange(int, ICollection{T})"/>
    public void InsertRange(int index, IEnumerable<T> items) => InsertRange(index, [..items]);

    /// <inheritdoc/>
    public void Clear()
    {
        var before = _innerList.ToImmutableArray();
        _innerList.Clear();
        // 索引取 0..旧长度-1：升序连续，且与 before 逐位对应
        RaiseUpdate(new ListItemClearedEventArgs<T>(Readonly, before, [..Enumerable.Range(0, before.Length)]));
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        var index = _innerList.IndexOf(item);
        if (index < 0) return false;
        _innerList.RemoveAt(index);
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, [item], [index]));
        return true;
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        var item = _innerList[index];
        _innerList.RemoveAt(index);
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, [item], [index]));
    }

    /// <summary>
    ///     移除指定范围内的元素，并发出<b>一次</b>携带这批元素及其原始索引的移除通知。
    ///     <br/>
    ///     Removes the elements in the given range and raises a <b>single</b> removal notification carrying those
    ///     elements together with their original indices.
    /// </summary>
    public void RemoveRange(int index, int count)
    {
        var removed = _innerList[index..(index + count)];
        // 升序连续，且与 removed 逐位对应
        var removedIndexes = Enumerable.Range(index, count).ToImmutableList();
        _innerList.RemoveRange(index, count);
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, removed, removedIndexes));
    }

    /// <summary>
    ///     移除所有满足条件的元素，并发出<b>一次</b>移除通知，被移除元素的索引按升序排列。
    ///     <br/>
    ///     Removes every element matching the predicate and raises a <b>single</b> removal notification whose
    ///     indices are in ascending order.
    /// </summary>
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

    /// <summary>
    ///     就地排序，并发出一次重排通知。
    ///     <br/>
    ///     Sorts in place and raises a single reorder notification.
    /// </summary>
    public void Sort()
    {
        _innerList.Sort();
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     按给定比较方式就地排序，并发出一次重排通知。
    ///     <br/>
    ///     Sorts in place using the given comparison and raises a single reorder notification.
    /// </summary>
    public void Sort(Comparison<T> comparison)
    {
        _innerList.Sort(comparison);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     按给定比较器就地排序，并发出一次重排通知。
    ///     <br/>
    ///     Sorts in place using the given comparer and raises a single reorder notification.
    /// </summary>
    public void Sort(IComparer<T> comparer)
    {
        _innerList.Sort(comparer);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     对指定范围就地排序，并发出一次重排通知。
    ///     <br/>
    ///     Sorts the given range in place and raises a single reorder notification.
    /// </summary>
    public void Sort(int index, int count, IComparer<T> comparer)
    {
        _innerList.Sort(index, count, comparer);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     就地反转整个列表，并发出一次重排通知。
    ///     <br/>
    ///     Reverses the whole list in place and raises a single reorder notification.
    /// </summary>
    public void Reverse()
    {
        _innerList.Reverse();
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     就地反转指定范围，并发出一次重排通知。
    ///     <br/>
    ///     Reverses the given range in place and raises a single reorder notification.
    /// </summary>
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

    /// <inheritdoc/>
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
