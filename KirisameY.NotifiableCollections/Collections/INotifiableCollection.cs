using System.Collections;

using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可读写、且可以订阅变更通知的集合。
///     <br/>
///     A mutable collection whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
public interface INotifiableCollection<T> : ICollection<T>, IReadOnlyNotifiableCollection<T>
{
    /// <summary>
    ///     元素个数。
    ///     <br/>
    ///     The number of elements.
    /// </summary>
    public new int Count { get; }

    int ICollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;

    bool ICollection<T>.IsReadOnly => false;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    #region Update

    /// <summary>
    ///     向集合中追加一批元素。
    ///     <br/>
    ///     Appends a batch of elements.
    /// </summary>
    [PublicAPI] void AddRange(IReadOnlyCollection<T> elements);

    /// <summary>
    ///     批量移除元素。
    ///     <br/>
    ///     Remove elements in bulk.
    /// </summary>
    /// <returns>
    ///     被成功移除的元素的集合。
    ///     <br/>
    ///     Collection of elements that were actually removed.
    /// </returns>
    [PublicAPI] IReadOnlyCollection<T> RemoveRange(IReadOnlyCollection<T> elements);

    /// <summary>
    ///     移除所有满足条件的元素。
    ///     <br/>
    ///     Removes every element matching the predicate.
    /// </summary>
    /// <returns>
    ///     被移除的项的集合。
    ///     <br/>
    ///     Collection of removed items.
    /// </returns>
    [PublicAPI] IReadOnlyCollection<T> RemoveAll(Predicate<T> predicate);

    /// <summary>
    ///     用指定的新元素替换旧的元素。
    ///     <br/>
    ///     Replace specified old element with given new element.
    /// </summary>
    /// <returns>
    ///     若成功找到并替换，返回 <c>true</c>，否则返回 <c>false</c>。
    ///     <br/>
    ///     <c>true</c> if found target element and replaced successfully, otherwise, <c>false</c>.
    /// </returns>
    [PublicAPI] bool Replace(T oldElement, T newElement);

    /// <summary>
    ///     用给定的一组替换指令批量对集合中的元素进行替换。
    ///     <br/>
    ///     Replace elements of the collection in bulk by with the specified replace commands.
    /// </summary>
    /// <returns>
    ///     包含一组替换结果的集合，其中每个元素的 <c>Replaced</c> 标志替换是否成功；
    ///     <c>Left</c> 为替换后处在集合之外的元素：当前者为 <c>true</c> 时为被替换的元素，反之为未成功加入的新元素。
    ///     <br/>
    ///     A collection of replacement results. For each element in the collection,
    ///     the <c>Replaced</c> flag indicates whether the replacement was successful,
    ///     while <c>Left</c> refers to elements that are no longer in the collection after the replacement:
    ///     when the former is <c>true</c>, it is the replaced element;
    ///     otherwise, it is a new element that was not successfully added.
    /// </returns>
    [PublicAPI] IReadOnlyCollection<(bool Replaced, T Left)> ReplaceRange(IReadOnlyCollection<(T Old, T New)> replaces);

    #endregion
}