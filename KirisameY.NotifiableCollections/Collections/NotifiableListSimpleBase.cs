using System.Collections.Immutable;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Data;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     用于简易实现 <see cref="INotifiableList{T}"/> 的基类型。
///     <br/>
///     Base type for implement <see cref="INotifiableList{T}"/> easily.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public abstract class NotifiableListSimpleBase<T> : INotifiableList<T>
{
    #region Abstract & Virtual

    /// <summary>
    ///     该列表的只读包装视图。
    ///     <br/>
    ///     Read-only wrapped view of this list.
    /// </summary>
    protected abstract IReadOnlyList<T> View { get; }

    /// <summary>
    ///     获取指定索引处的项。
    ///     <br/>
    ///     Get the item specified by the given index.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     索引超出合法范围。
    ///     <br/>
    ///     Index is out of legal range.
    /// </exception>
    protected virtual T GetItem(int index) => View[index];

    /// <summary>
    ///     从给定的起始索引处获得指定数量个连续的项，作为只读列表返回。
    ///     <br/>
    ///     Get the specified number of consecutive items starting from
    ///     the given starting index and return them as a read-only list.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     给定的范围有超出列表的部分，或 <paramref name="count"/> 小于0。
    ///     <br/>
    ///     The specified range extends beyond the list,
    ///     or <paramref name="count"/> is less than 0.
    /// </exception>
    protected virtual IReadOnlyList<T> GetItems(int index, int count)
    {
        return Enumerable.Range(index, count)
                         .Select(GetItem)
                         .ToImmutableList();
    }

    /// <summary>
    ///     将指定索引处的项设置为给定的 <paramref name="item"/>。
    ///     <br/>
    ///     Set the item specified by the given index to given <paramref name="item"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     索引超出合法范围。
    ///     <br/>
    ///     Index is out of legal range.
    /// </exception>
    protected abstract void SetItem(int index, T item);

    /// <summary>
    ///     <para>
    ///         向指定索引处插入一个项，其后的项将被后移。<br/>
    ///         若 <paramref name="index"/> 值为当前列表长度，将该项追加至末尾。
    ///     </para>
    ///     <para>
    ///         Insert an item at the specified index;
    ///         the items following it will shifted back.
    ///         <br/>
    ///         If the value of <paramref name="index"/> is equal to the
    ///         length of current list, append this item to the end.
    ///     </para>
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     索引超出合法范围。
    ///     <br/>
    ///     Index is out of legal range.
    /// </exception>
    protected abstract void InsertItem(int index, T item);

    /// <summary>
    ///     <para>
    ///         向指定索引处插入任意个项，其后的项将被后移。<br/>
    ///         若 <paramref name="index"/> 值为当前列表长度，将该项追加至末尾。
    ///     </para>
    ///     <para>
    ///         Insert items at the specified index;
    ///         the items following them will shifted back.
    ///         <br/>
    ///         If the value of <paramref name="index"/> is equal to the
    ///         length of current list, append this item to the end.
    ///     </para>
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     索引超出合法范围。
    ///     <br/>
    ///     Index is out of legal range.
    /// </exception>
    protected virtual void InsertItems(int index, IEnumerable<T> items)
    {
        foreach (var (i, item) in items.Index())
        {
            InsertItem(index + i, item);
        }
    }

    /// <summary>
    ///     从指定索引处移除一个项，其后的项将被前移。
    ///     <br/>
    ///     Remove an item at the specified index;
    ///     the items following it will shifted front.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     索引超出合法范围。
    ///     <br/>
    ///     Index is out of legal range.
    /// </exception>
    protected abstract void RemoveItem(int index);

    /// <summary>
    ///     从指定索引处移除给定数量个项，其后的项将被前移。
    ///     <br/>
    ///     Remove the specified number of consecutive items
    ///     starting from the given starting index;
    ///     the items following them will shifted front.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     给定的范围有超出列表的部分，或 <paramref name="count"/> 小于0。
    ///     <br/>
    ///     The specified range extends beyond the list,
    ///     or <paramref name="count"/> is less than 0.
    /// </exception>
    protected virtual void RemoveItems(int index, int count)
    {
        for (int i = count - 1; i >= 0; i--)
        {
            RemoveItem(index + i);
        }
    }

    /// <summary>
    ///     移除列表里的所有项。
    ///     <br/>
    ///     Remove all items in the list.
    /// </summary>
    protected virtual void ClearItems()
    {
        for (int i = View.Count - 1; i >= 0; i--)
        {
            RemoveItem(i);
        }
    }

    /// <summary>
    ///     使用给定的比较器对指定范围内的项进行排序。
    ///     <br/>
    ///     Sort item in the specified range by the given comparer.
    /// </summary>
    ///     /// <exception cref="ArgumentException">
    ///     给定的范围有超出列表的部分，或 <paramref name="count"/> 小于0。
    ///     <br/>
    ///     The specified range extends beyond the list,
    ///     or <paramref name="count"/> is less than 0.
    /// </exception>
    protected virtual void SortItems(int index, int count, IComparer<T> comparer)
    {
        var array = GetItems(index, count).ToArray();
        array.Sort(comparer);
        for (int i = 0; i < count; i++)
        {
            SetItem(index + i, array[i]);
        }
    }

    /// <summary>
    ///     反转给定范围内的项的顺序。
    ///     <br/>
    ///     Reverse the order of items in the specified range.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     给定的范围有超出列表的部分，或 <paramref name="count"/> 小于0。
    ///     <br/>
    ///     The specified range extends beyond the list,
    ///     or <paramref name="count"/> is less than 0.
    /// </exception>
    protected virtual void ReverseItems(int index, int count)
    {
        foreach (var (i, item) in GetItems(index, count).Reverse().Index())
        {
            SetItem(index + i, item);
        }
    }

    #endregion


    #region Reading

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => View.GetEnumerator();

    /// <inheritdoc/>
    public int IndexOf(T item) => View.Index().FirstOrDefault(
        t => EqualityComparer<T>.Default.Equals(t.Item, item),
        (-1, default!)
    ).Index;

    /// <inheritdoc cref="ICollection{T}.Count"/>
    public int Count => View.Count;

    /// <inheritdoc />
    public bool Contains(T item) => View.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        var count = View.Count;
        for (int i = 0; i < count; i++)
        {
            array[arrayIndex + i] = View[i];
        }
    }

    #endregion


    #region Writing

    /// <inheritdoc cref="IList{T}.this"/>
    public T this[int index]
    {
        get => GetItem(index);
        set
        {
            var old = GetItem(index);
            SetItem(index, value);
            OnListUpdated(new ListItemReplacedEventArgs<T>(View, [old], [value], [index]));
        }
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
        var target = View.Count;
        InsertItem(target, item);
        OnListUpdated(new ListItemAddedEventArgs<T>(View, [item], [target]));
    }

    /// <inheritdoc/>
    public void AddRange(IReadOnlyCollection<T> items)
    {
        var fromIndex = View.Count;
        InsertItems(fromIndex, items);
        OnListUpdated(new ListItemAddedEventArgs<T>(View, [..items], [..Enumerable.Range(fromIndex, items.Count)]));
    }

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        InsertItem(index, item);
        OnListUpdated(new ListItemAddedEventArgs<T>(View, [item], [index]));
    }

    /// <inheritdoc/>
    public void InsertRange(int index, IReadOnlyCollection<T> items)
    {
        InsertItems(index, items);
        OnListUpdated(new ListItemAddedEventArgs<T>(View, [..items], [..Enumerable.Range(index, items.Count)]));
    }


    /// <inheritdoc/>
    public void MultiInsert(IEnumerable<IItemWithIndex<T>> items)
    {
        var itemsList = items.ToImmutableArray();
        foreach (var (i, item) in itemsList.Index())
        {
            try
            {
                if (i > 0 && item.Index <= itemsList[i - 1].Index)
                {
                    throw new ArgumentException("Items to insert should be in ascending order and have no duplicated index");
                }
                InsertItem(item.Index, item.Item);
            }
            catch (ArgumentException)
            {
                // reverse
                for (int j = i - 1; j >= 0; j--)
                {
                    RemoveItem(itemsList[j].Index);
                }
                throw;
            }
        }
        OnListUpdated(new ListItemAddedEventArgs<T>(View, itemsList.ItemsView, itemsList.IndexesView));
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> Clear()
    {
        var before = View.ToImmutableList();
        ClearItems();
        // 索引取 0..旧长度-1：升序连续，且与 before 逐位对应
        OnListUpdated(new ListItemClearedEventArgs<T>(View, before, [..Enumerable.Range(0, before.Count)]));
        return before;
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;
        RemoveItem(index);
        OnListUpdated(new ListItemRemovedEventArgs<T>(View, [item], [index]));
        return true;
    }

    /// <inheritdoc/>
    public T RemoveAt(int index)
    {
        var item = View[index];
        RemoveItem(index);
        OnListUpdated(new ListItemRemovedEventArgs<T>(View, [item], [index]));
        return item;
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> RemoveRange(int index, int count)
    {
        var removed = GetItems(index, count);
        // 升序连续，且与 removed 逐位对应
        var removedIndexes = Enumerable.Range(index, count).ToImmutableList();
        RemoveItems(index, count);
        OnListUpdated(new ListItemRemovedEventArgs<T>(View, removed, removedIndexes));
        return removed;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<T> RemoveRange(IReadOnlyCollection<T> items)
    {
        var removeInfo = items.GroupBy(
            item => item, (item, c) => KeyValuePair.Create(Optional.FromNullable(item), c.Count())
        ).ToDictionary();
        List<int> removes = new(capacity: items.Count);
        foreach (var (i, item) in View.Index()) // 升序遍历保证序列顺序
        {
            if (removeInfo.Count == 0) break;
            var key = Optional.FromNullable(item);
            if (!removeInfo.TryGetValue(key, out var c)) continue;

            if (c > 1) removeInfo[key] = c - 1;
            else removeInfo.Remove(key);
            removes.Add(i);
        }
        return MultiRemove(removes);
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> MultiRemove(IEnumerable<int> indexes)
    {
        var indexesList = indexes.ToArray();
        var removed = new T[indexesList.Length];
        foreach (var (i, index) in indexesList.Index().Reverse())
        {
            try
            {
                if (i > 0 && index <= indexesList[i - 1])
                    throw new ArgumentException("Items to remove should be in ascending order and have no duplicated index");
                removed[i] = View[index];
                RemoveItem(index);
            }
            catch (ArgumentException)
            {
                for (int j = i + 1; j < indexesList.Length; j++)
                {
                    InsertItem(indexesList[j], removed[j]);
                }
                throw;
            }
        }
        var finalRemoved = removed.ToImmutableList();
        OnListUpdated(new ListItemRemovedEventArgs<T>(View, finalRemoved, indexesList.ToImmutableList()));
        return finalRemoved;
    }

    /// <inheritdoc/>
    public T? RemoveFirst(Predicate<T> predicate)
    {
        var (i, item) = View.Index().FirstOrDefault(t => predicate.Invoke(t.Item), (-1, default!));
        if (i < 0) return default;
        RemoveItem(i);
        OnListUpdated(new ListItemRemovedEventArgs<T>(View, [item], [i]));
        return item;
    }

    /// <inheritdoc/>
    public T? RemoveLast(Predicate<T> predicate)
    {
        var (i, item) = View.Index().LastOrDefault(t => predicate.Invoke(t.Item), (-1, default!));
        if (i < 0) return default;
        RemoveItem(i);
        OnListUpdated(new ListItemRemovedEventArgs<T>(View, [item], [i]));
        return item;
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> RemoveAll(Predicate<T> predicate)
    {
        // 顺着原索引扫一遍再过滤，天然升序
        var indexes = View.Select((item, index) => predicate.Invoke(item) ? index : -1)
                          .Where(i => i >= 0).ToImmutableList();
        // 内层倒序 RemoveAt 是必须的（否则删完前面的索引就错位了），
        // 外层再 Reverse 一次把 items 翻回升序，好跟 indexes 逐位对齐
        var items = indexes.Reverse().Select(i =>
        {
            var item = View[i];
            RemoveItem(i);
            return item;
        }).Reverse().ToImmutableList();
        OnListUpdated(new ListItemRemovedEventArgs<T>(View, items, indexes));
        return items;
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> MultiReplace(IEnumerable<IItemWithIndex<T>> items)
    {
        var itemsList = items.ToImmutableList();
        var oldItems = new T[itemsList.Count];
        foreach (var (i, item) in itemsList.Index())
        {
            try
            {
                if (i > 0 && item.Index <= itemsList[i - 1].Index)
                {
                    throw new ArgumentException("Items to replace should be in ascending order and have no duplicated index");
                }
                oldItems[i] = View[item.Index];
                SetItem(item.Index, item.Item);
            }
            catch (ArgumentException)
            {
                // reverse
                for (int j = i - 1; j >= 0; j--)
                {
                    SetItem(itemsList[j].Index, oldItems[j]);
                }
                throw;
            }
        }
        var finalOldItems = oldItems.ToImmutableList();
        OnListUpdated(new ListItemReplacedEventArgs<T>(View, finalOldItems, itemsList.ItemsView, itemsList.IndexesView));
        return finalOldItems;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<(bool Replaced, T Left)> ReplaceRange(IReadOnlyCollection<(T Old, T New)> replaces)
    {
        var replaceInfo = replaces.GroupBy(
            t => t.Old,
            (old, c) => KeyValuePair.Create(Optional.FromNullable(old), c.Select(t => t.New).Reverse().ToList())
        ).ToDictionary();
        List<IItemWithIndex<T>> replaceList = new(capacity: replaces.Count);

        foreach (var (index, item) in View.Index()) // 升序遍历保证序列顺序
        {
            if (replaceInfo.Count == 0) break;
            var key = Optional.FromNullable(item);
            if (!replaceInfo.TryGetValue(key, out var news)) continue;
            // 从字典中移除匹配项
            var @new = news[^1];
            news.RemoveAt(news.Count - 1);
            if (news.Count == 0) replaceInfo.Remove(key);

            replaceList.Add(@new.WithIndex(index));
        }

        return
        [
            ..MultiReplace(replaceList).Select(o => (true, o)),
            ..replaceInfo.SelectMany(p => ((IEnumerable<T>)p.Value).Reverse()).Select(n => (false, n))
        ];
    }

    /// <inheritdoc/>
    public void Move(int index, int to)
    {
        if (index == to) return;

        var item = View[index];
        RemoveItem(index);
        try { InsertItem(to, item); }
        catch (ArgumentException)
        {
            InsertItem(index, item);
            throw;
        }

        OnListUpdated(new ListItemMovedEventArgs<T>(View, [item], [index], [to]));
    }

    /// <inheritdoc/>
    public void MoveRange(int index, int count, int to)
    {
        if (index == to || count == 0) return;

        var items = GetItems(index, count);
        RemoveItems(index, count);
        try { InsertItems(to, items); }
        catch (ArgumentException)
        {
            InsertItems(index, items);
            throw;
        }

        if (index > to) items = [..items.Reverse()];
        var (fromIndexes, toIndexes) = index < to
            ? (Enumerable.Repeat(index, count), Enumerable.Repeat(to + count - 1, count))
            : (Enumerable.Repeat(index + count - 1, count), Enumerable.Repeat(to, count));
        OnListUpdated(new ListItemMovedEventArgs<T>(View, items, [..fromIndexes], [..toIndexes]));
    }

    /// <inheritdoc/>
    public void MultiMove(IEnumerable<(int From, int To)> steps)
    {
        var stepsList = steps.ToArray();
        var movedItems = new T[stepsList.Length];
        foreach (var (i, (from, to)) in stepsList.Index())
        {
            try
            {
                var item = View[from];
                RemoveItem(from);
                // if insert failed, rollback this turn
                try { InsertItem(to, item); }
                catch (ArgumentException)
                {
                    InsertItem(from, item);
                    throw;
                }
                movedItems[i] = item;
            }
            catch (ArgumentException)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var (from1, to1) = stepsList[j];
                    var item1 = View[to1];
                    RemoveItem(to1);
                    InsertItem(from1, item1);
                }
                throw;
            }
        }

        OnListUpdated(
            new ListItemMovedEventArgs<T>(
                View, [..movedItems],
                [..stepsList.Select(t => t.From)],
                [..stepsList.Select(t => t.To)]
            )
        );
    }

    /// <inheritdoc/>
    public void Sort(IComparer<T> comparer)
    {
        var snapshot = View.ToImmutableList();
        SortItems(0, View.Count, comparer);
        OnListUpdated(ListSortedEventArgs<T>.From(View, snapshot));
    }

    /// <inheritdoc/>
    public void Sort(int index, int count, IComparer<T> comparer)
    {
        var snapshot = View.ToArray();
        SortItems(index, count, comparer);
        OnListUpdated(ListSortedEventArgs<T>.From(View, snapshot));
    }

    /// <inheritdoc/>
    public void Reverse()
    {
        if (View.Count == 0) return;
        ReverseItems(0, View.Count);
        OnListUpdated(new ListSortedEventArgs<T>(
                          View, [..View.SkipLast(1)],
                          [..Enumerable.Repeat(View.Count - 1, View.Count - 1)],
                          [..Enumerable.Range(0, View.Count - 1)]
                      ));
    }

    /// <inheritdoc/>
    public void Reverse(int index, int count)
    {
        if (count == 0) return;

        ReverseItems(index, count);
        OnListUpdated(new ListSortedEventArgs<T>(
                          View, GetItems(index, count - 1),
                          [..Enumerable.Repeat(index + count - 1, count - 1)],
                          [..Enumerable.Range(index, count - 1)]
                      ));
    }

    #endregion


    #region Event

    /// <summary>
    ///     引发 <see cref="ListUpdated"/> 事件。
    ///     <br/>
    ///     Raise the <see cref="ListUpdated"/> event.
    /// </summary>
    /// <remarks>
    ///     覆写该方法时，应当调用 <c>base.OnListUpdated(e)</c> 以保证事件处理器能够接收到通知。
    ///     <br/>
    ///     When overriding, it's recommended to call <c>base.OnListUpdated(e)</c>
    ///     to ensure that event handlers can receive the notification.
    /// </remarks>
    [PublicAPI] protected virtual void OnListUpdated(ListUpdateEventArgs<T> args)
    {
        RaiseUpdate(args);
    }

    private ImmutableList<EventHandler<ListUpdateEventArgs<T>>> _updatedEventHandlers = [];

    private void RaiseUpdate(ListUpdateEventArgs<T> args)
    {
        foreach (var eventHandler in _updatedEventHandlers) eventHandler.Invoke(this, args);
    }

    /// <inheritdoc/>
    public event EventHandler<ListUpdateEventArgs<T>> ListUpdated
    {
        add => _updatedEventHandlers = _updatedEventHandlers.Add(value);
        remove => _updatedEventHandlers = _updatedEventHandlers.Remove(value);
    }

    #endregion
}