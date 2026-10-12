using System.Collections;
using System.Collections.Immutable;

using KirisameY.NotifiableCollections.Data;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

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
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [item], [_innerList.Count - 1]));
    }

    /// <summary>
    ///     在末尾追加一批元素。
    ///     <br/>
    ///     Appends a batch of elements.
    /// </summary>
    public void AddRange(ICollection<T> items)
    {
        var fromIndex = _innerList.Count;
        _innerList.AddRange(items);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [..items], [..Enumerable.Range(fromIndex, items.Count)]));
    }

    /// <inheritdoc cref="AddRange(ICollection{T})"/>
    public void AddRange(IEnumerable<T> items) => AddRange([..items]);

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        _innerList.Insert(index, item);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [item], [index]));
    }

    /// <summary>
    ///     从指定位置起插入一批元素。
    ///     <br/>
    ///     Inserts a batch of elements at the given position.
    /// </summary>
    public void InsertRange(int index, ICollection<T> items)
    {
        _innerList.InsertRange(index, items);
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, [..items], [..Enumerable.Range(index, items.Count)]));
    }

    /// <inheritdoc cref="InsertRange(int, ICollection{T})"/>
    public void InsertRange(int index, IEnumerable<T> items) => InsertRange(index, [..items]);

    /// <summary>
    ///     将一组指定索引的项插入列表中。
    ///     <br/>
    ///     Inserts a batch of items with given index into the list.
    /// </summary>
    /// <param name="items">
    ///     要加入的项和要加入至的索引的集合，应当按照索引被升序排列且没有重复索引项。
    ///     <br/>
    ///     A collection of items and their indexes to insert, should be ascending ordered by indexes and have no duplicated indexes.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="items"/> 中存在非法或重复的的索引项，或未按照索引被升序排列。
    ///     <br/>
    ///     <paramref name="items"/> containes invalid or duplicated index, or is not ascending ordered by indexes.
    /// </exception>
    public void MultiInsert(IReadOnlyList<IItemWithIndex<T>> items)
    {
        foreach (var (i, item) in items.Index())
        {
            try
            {
                if (i > 0 && item.Index <= items[i - 1].Index)
                {
                    throw new ArgumentException("Items to insert should be in ascending order and have no duplicated index");
                }
                _innerList.Insert(item.Index, item.Item);
            }
            catch (ArgumentException)
            {
                // reverse
                for (int j = i - 1; j >= 0; j--)
                {
                    _innerList.RemoveAt(items[j].Index);
                }
                throw;
            }
        }
        RaiseUpdate(new ListItemAddedEventArgs<T>(Readonly, items.ItemsView, items.IndexesView));
    }

    /// <inheritdoc cref="MultiInsert(IReadOnlyList{IItemWithIndex{T}})"/>
    public void MultiInsert(IEnumerable<IItemWithIndex<T>> items) => MultiInsert(items.ToImmutableList());

    /// <summary>
    ///     将一组指定索引的项插入列表中。
    ///     <br/>
    ///     Inserts a batch of items with given index into the list.
    /// </summary>
    /// <param name="indexes">
    ///     要插入到的目标索引的集合，应当被升序排列且没有重复索引项。
    ///     <br/>
    ///     A collection of target indexes to add to, should be ascending ordered and have no duplicated indexes.
    /// </param>
    /// <param name="items">
    ///     要添加的项的集合，应当与 <paramref name="indexes"/> 逐位对齐。
    ///     <br/>
    ///     A collection of items to add, should be aligned in the same order with <paramref name="indexes"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="indexes"/> 中存在非法或重复的的索引项或未按照索引被升序排列，或长度与 <paramref name="items"/> 不匹配。
    ///     <br/>
    ///     <paramref name="indexes"/> containes invalid or duplicated index or is not ascending ordered by indexes,
    ///     or length mismatched with <paramref name="items"/>.
    /// </exception>
    public void MultiInsert(IEnumerable<int> indexes, IEnumerable<T> items) => MultiInsert(indexes.ZipChecked(items, ItemWithIndex.From));

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
    ///     移除指定范围内的元素。
    ///     <br/>
    ///     Removes the elements in the given range.
    /// </summary>
    public IReadOnlyList<T> RemoveRange(int index, int count)
    {
        var removed = _innerList[index..(index + count)].ToImmutableList();
        // 升序连续，且与 removed 逐位对应
        var removedIndexes = Enumerable.Range(index, count).ToImmutableList();
        _innerList.RemoveRange(index, count);
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, removed, removedIndexes));
        return removed;
    }

    /// <summary>
    ///     从列表中指定的一组指定的索引处移除相应的值。
    ///     <br/>
    ///     Remove the elements in a given batch of indexes.
    /// </summary>
    /// <param name="indexes">
    ///     要移除的索引的集合，应当按照索引被升序排列且没有重复索引项。
    ///     <br/>
    ///     A collection of indexes to remove, should be ascending ordered and have no duplicated indexes.
    /// </param>
    /// <returns>
    ///     被移除的项的列表，与 <paramref name="indexes"/> 逐位对齐。
    ///     <br/>
    ///     A list of removed items, aligned in the same order with <paramref name="indexes"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="indexes"/> 中存在非法或重复的的索引项，或未被升序排列。
    ///     <br/>
    ///     <paramref name="indexes"/> containes invalid or duplicated index, or is not ascending ordered.
    /// </exception>
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
                removed[i] = _innerList[index];
                _innerList.RemoveAt(index);
            }
            catch (ArgumentException)
            {
                for (int j = i + 1; j < indexesList.Length; j++)
                {
                    _innerList.Insert(indexesList[j], removed[j]);
                }
                throw;
            }
        }
        var finalRemoved = removed.ToImmutableList();
        RaiseUpdate(new ListItemRemovedEventArgs<T>(Readonly, finalRemoved, indexesList.ToImmutableList()));
        return finalRemoved;
    }

    /// <summary>
    ///     移除所有满足条件的元素。
    ///     <br/>
    ///     Removes every element matching the predicate.
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
    ///     将一组指定索引的项替换至列表中。
    ///     <br/>
    ///     Replace list items with a batch of items with given index.
    /// </summary>
    /// <param name="items">
    ///     要替换的项和相应索引的集合，应当按照索引被升序排列且没有重复索引项。
    ///     <br/>
    ///     A collection of items and their indexes to replace, should be ascending ordered by indexes and have no duplicated indexes.
    /// </param>
    /// <returns>
    ///     被替换的项的列表，与 <paramref name="items"/> 逐位对齐。
    ///     <br/>
    ///     A list of replaced items, aligned in the same order with <paramref name="items"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="items"/> 中存在非法或重复的的索引项，或未按照索引被升序排列。
    ///     <br/>
    ///     <paramref name="items"/> containes invalid or duplicated index, or is not ascending ordered by indexes.
    /// </exception>
    public IReadOnlyList<T> MultiReplace(IReadOnlyList<IItemWithIndex<T>> items)
    {
        var oldItems = new T[items.Count];
        foreach (var (i, item) in items.Index())
        {
            try
            {
                if (i > 0 && item.Index <= items[i - 1].Index)
                {
                    throw new ArgumentException("Items to replace should be in ascending order and have no duplicated index");
                }
                oldItems[i]            = _innerList[item.Index];
                _innerList[item.Index] = item.Item;
            }
            catch (ArgumentException)
            {
                // reverse
                for (int j = i - 1; j >= 0; j--)
                {
                    _innerList[items[j].Index] = oldItems[j];
                }
                throw;
            }
        }
        var finalOldItems = oldItems.ToImmutableList();
        RaiseUpdate(new ListItemReplacedEventArgs<T>(Readonly, finalOldItems, items.ItemsView, items.IndexesView));
        return finalOldItems;
    }

    /// <inheritdoc cref="MultiReplace(IReadOnlyList{IItemWithIndex{T}})"/>
    public IReadOnlyList<T> MultiReplace(IEnumerable<IItemWithIndex<T>> items) => MultiReplace(items.ToImmutableList());

    /// <summary>
    ///     将一组指定索引的项替换至列表中。
    ///     <br/>
    ///     Replace list items with a batch of items with given index.
    /// </summary>
    /// <param name="indexes">
    ///     要替换的项所在索引的集合，应当被升序排列且没有重复索引项。
    ///     <br/>
    ///     A collection of indexes of items to replace, should be ascending ordered and have no duplicated indexes.
    /// </param>
    /// <param name="items">
    ///     要替换的项的集合，应当与 <paramref name="indexes"/> 逐位对齐。
    ///     <br/>
    ///     A collection of items to replace, should be aligned in the same order with <paramref name="indexes"/>.
    /// </param>
    /// <returns>
    ///     被替换的项的列表，与 <paramref name="items"/> 逐位对齐。
    ///     <br/>
    ///     A list of replaced items, aligned in the same order with <paramref name="items"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="indexes"/> 中存在非法或重复的的索引项或未按照索引被升序排列，或长度与 <paramref name="items"/> 不匹配。
    ///     <br/>
    ///     <paramref name="indexes"/> containes invalid or duplicated index or is not ascending ordered by indexes,
    ///     or length mismatched with <paramref name="items"/>.
    /// </exception>
    public IReadOnlyList<T> MultiReplace(IEnumerable<int> indexes, IEnumerable<T> items) => MultiReplace(indexes.ZipChecked(items, ItemWithIndex.From));

    /// <summary>
    ///     将指定索引的列表项移动至另一索引。
    ///     <br/>
    ///     Move the list item at the specified index to another index.
    /// </summary>
    public void Move(int index, int to)
    {
        var item = _innerList[index];
        _innerList.RemoveAt(index);
        _innerList.Insert(to, item);

        RaiseUpdate(new ListItemMovedEventArgs<T>(Readonly, [item], [index], [to]));
    }

    // todo: sort remake
    // /// <summary>
    // ///     按照给定的一组源索引与目标索引移动列表中的元素。
    // ///     <br/>
    // ///     Move items by a set of given origin and target index.
    // /// </summary>
    // /// <exception cref="ArgumentException">
    // ///     <paramref name="steps"/> 中存在非法的索引项。
    // ///     <br/>
    // ///     <paramref name="steps"/> contains invalid index.
    // /// </exception>
    // public void MultiMove(IEnumerable<(int From, int To)> steps)
    // {
    //     var stepsList = steps.ToArray();
    //     var movedItems = new T[stepsList.Length];
    //     foreach (var (i, (from, to)) in stepsList.Index())
    //     {
    //         try
    //         {
    //             var item = _innerList[from];
    //             _innerList.RemoveAt(from);
    //             // if insert failed, rollback this turn
    //             try { _innerList.Insert(to, item); }
    //             catch (ArgumentException)
    //             {
    //                 _innerList.Insert(from, item);
    //                 throw;
    //             }
    //             movedItems[i] = item;
    //         }
    //         catch (ArgumentException)
    //         {
    //             for (int j = i - 1; j >= 0; j--)
    //             {
    //                 var (from1, to1) = stepsList[j];
    //                 var item1 = _innerList[to1];
    //                 _innerList.RemoveAt(to1);
    //                 _innerList.Insert(from1, item1);
    //             }
    //             throw;
    //         }
    //     }
    //
    //     RaiseUpdate(
    //         new ListItemMovedEventArgs<T>(
    //             Readonly, [..movedItems],
    //             [..stepsList.Select(t => t.From)],
    //             [..stepsList.Select(t => t.To)]
    //         )
    //     );
    // }

    /// <summary>
    ///     就地排序。
    ///     <br/>
    ///     Sorts in place.
    /// </summary>
    public void Sort()
    {
        _innerList.Sort();
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     按给定比较方式就地排序。
    ///     <br/>
    ///     Sorts in place using the given comparison.
    /// </summary>
    public void Sort(Comparison<T> comparison)
    {
        _innerList.Sort(comparison);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     按给定比较器就地排序。
    ///     <br/>
    ///     Sorts in place using the given comparer.
    /// </summary>
    public void Sort(IComparer<T> comparer)
    {
        _innerList.Sort(comparer);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     对指定范围就地排序。
    ///     <br/>
    ///     Sorts the given range in place.
    /// </summary>
    public void Sort(int index, int count, IComparer<T> comparer)
    {
        _innerList.Sort(index, count, comparer);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     就地反转整个列表。
    ///     <br/>
    ///     Reverses the whole list in place.
    /// </summary>
    public void Reverse()
    {
        _innerList.Reverse();
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    /// <summary>
    ///     就地反转指定范围。
    ///     <br/>
    ///     Reverses the given range in place.
    /// </summary>
    public void Reverse(int index, int count)
    {
        _innerList.Reverse(index, count);
        RaiseUpdate(new ListSortedEventArgs<T>(Readonly));
    }

    #endregion


    //event

    private ImmutableList<EventHandler<ListUpdateEventArgs<T>>> _updatedEventHandlers = [];

    private void RaiseUpdate(ListUpdateEventArgs<T> args)
    {
        foreach (var eventHandler in _updatedEventHandlers)
        {
            eventHandler.Invoke(this, args);
        }
    }

    /// <inheritdoc/>
    public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated
    {
        add
        {
            if (value is not null) _updatedEventHandlers = _updatedEventHandlers.Add(value);
        }
        remove
        {
            if (value is not null) _updatedEventHandlers = _updatedEventHandlers.Remove(value);
        }
    }
}