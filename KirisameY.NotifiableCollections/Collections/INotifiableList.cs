using System.Collections;

using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可读写、且可以订阅变更通知的列表。在 <see cref="IList{T}"/> 的基础上，
///     加入了一系列批量操作 API，并会在发生变化时把改动通过 <see cref="IListUpdateNotifier{T}.ListUpdated"/> 事件进行通知。
///     <br/>
///     A mutable list whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public interface INotifiableList<T> : INotifiableCollection<T>, IList<T>, IReadOnlyNotifiableList<T>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="INotifiableCollection{T}.Count"/>
    public new int Count { get; }

    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    ///     按索引读写元素。
    ///     <br/>
    ///     Reads or writes an element by its index.
    /// </summary>
    /// <remarks>
    ///     赋值会发出一次替换通知。
    ///     <br/>
    ///     Assigning raises a single replacement notification.
    /// </remarks>
    public new T this[int index] { get; set; }


    T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }
    T IReadOnlyList<T>.this[int index] => this[index];

    int ICollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;

    #region Updating

    /// <summary>
    ///     从指定位置起插入一批元素。
    ///     <br/>
    ///     Inserts a batch of elements at the given position.
    /// </summary>
    [PublicAPI] void InsertRange(int index, IReadOnlyCollection<T> items);

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
    [PublicAPI] void MultiInsert(IEnumerable<IItemWithIndex<T>> items);

    /// <summary>
    ///     从列表中移除所有项。
    ///     <br/>
    ///     Remove all items from the list.
    /// </summary>
    /// <returns>
    ///     被移除的项的列表。
    ///     <br/>
    ///     List of removed items.
    /// </returns>
    [PublicAPI] new IReadOnlyList<T> Clear();

    [PublicAPI] void ICollection<T>.Clear() => Clear();

    /// <summary>
    ///     移除指定位置的列表项。
    ///     <br/>
    ///     Remove the list item at the specified index.
    /// </summary>
    /// <returns>
    ///     被移除的项。
    ///     <br/>
    ///     Removed item.
    /// </returns>
    [PublicAPI] new T RemoveAt(int index);

    [PublicAPI] void IList<T>.RemoveAt(int index) => RemoveAt(index);

    /// <summary>
    ///     移除指定范围内的元素。
    ///     <br/>
    ///     Removes the elements in the given range.
    /// </summary>
    /// <returns>
    ///     被移除的项的列表。
    ///     <br/>
    ///     List of removed items.
    /// </returns>
    [PublicAPI] IReadOnlyList<T> RemoveRange(int index, int count);

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
    [PublicAPI] IReadOnlyList<T> MultiRemove(IEnumerable<int> indexes);

    IReadOnlyCollection<T> INotifiableCollection<T>.RemoveRange(IReadOnlyCollection<T> elements) =>
        MultiRemove(elements.Select(IndexOf).Where(i => i > 0));

    /// <summary>
    ///     移除第一个满足条件的元素。
    ///     <br/>
    ///     Removes first element matching the predicate.
    /// </summary>
    /// <returns>
    ///     被移除的项。
    ///     <br/>
    ///     Removed item.
    /// </returns>
    [PublicAPI] T? RemoveFirst(Predicate<T> predicate);

    /// <summary>
    ///     移除最后一个满足条件的元素。
    ///     <br/>
    ///     Removes last element matching the predicate.
    /// </summary>
    /// <returns>
    ///     被移除的项。
    ///     <br/>
    ///     Removed item.
    /// </returns>
    [PublicAPI] T? RemoveLast(Predicate<T> predicate);

    /// <summary>
    ///     移除所有满足条件的项。
    ///     <br/>
    ///     Removes every item matching the predicate.
    /// </summary>
    /// <returns>
    ///     被移除的项的列表。
    ///     <br/>
    ///     List of removed items.
    /// </returns>
    [PublicAPI] new IReadOnlyList<T> RemoveAll(Predicate<T> predicate);

    IReadOnlyCollection<T> INotifiableCollection<T>.RemoveAll(Predicate<T> predicate) => RemoveAll(predicate);

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
    [PublicAPI] IReadOnlyList<T> MultiReplace(IEnumerable<IItemWithIndex<T>> items);

    bool INotifiableCollection<T>.Replace(T oldElement, T newElement)
    {
        if (IndexOf(oldElement) is not (>= 0 and var i)) return false;
        this[i] = newElement;
        return true;
    }

    IReadOnlyCollection<(bool Replaced, T Left)> INotifiableCollection<T>.ReplaceRange(IReadOnlyCollection<(T Old, T New)> replaces)
    {
        List<(bool Replaced, T Left)> result = new(capacity: replaces.Count);
        List<IItemWithIndex<T>> replace = new(capacity: replaces.Count);
        foreach (var (index, item) in replaces.Select(r => (IndexOf(r.Old), r.New)))
        {
            if (index <= 0) result.Add((false, item));
            else replace.Add(item.WithIndex(index));
        }
        var oldItems = MultiReplace(replace);
        result.AddRange(oldItems.Select(item => (true, item)));
        return [..result];
    }

    /// <summary>
    ///     将指定索引的列表项移动至另一索引。
    ///     <br/>
    ///     Move the list item at the specified index to another index.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     源索引或目标索引不合法。
    ///     <br/>
    ///     Origin index or target index is invalid.
    /// </exception>
    [PublicAPI] void Move(int index, int to);

    /// <summary>
    ///     将指定范围内的列表项移动至从另一索引起始处。
    ///     <br/>
    ///     Move the list items at the given range to a position starting from another index.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     源范围或目标索引不合法。
    ///     <br/>
    ///     Origin range or target index is invalid.
    /// </exception>
    [PublicAPI] void MoveRange(int index, int count, int to);

    /// <summary>
    ///     按照给定的一组源索引与目标索引移动列表中的元素。
    ///     <br/>
    ///     Move items by a set of given origin and target index.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     <paramref name="steps"/> 中存在非法的索引项。
    ///     <br/>
    ///     <paramref name="steps"/> contains invalid index.
    /// </exception>
    [PublicAPI] void MultiMove(IEnumerable<(int From, int To)> steps);

    /// <summary>
    ///     按给定比较器就地排序。
    ///     <br/>
    ///     Sorts in place using the given comparer.
    /// </summary>
    [PublicAPI] void Sort(IComparer<T> comparer);

    /// <summary>
    ///     对指定范围就地排序。
    ///     <br/>
    ///     Sorts the given range in place.
    /// </summary>
    [PublicAPI] void Sort(int index, int count, IComparer<T> comparer);

    /// <summary>
    ///     就地反转整个列表。
    ///     <br/>
    ///     Reverses the whole list in place.
    /// </summary>
    [PublicAPI] void Reverse();

    /// <summary>
    ///     就地反转指定范围。
    ///     <br/>
    ///     Reverses the given range in place.
    /// </summary>
    [PublicAPI] void Reverse(int index, int count);

    #endregion
}