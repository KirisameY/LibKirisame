namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     <see cref="INotifiableList{T}"/> 的默认实现。
///     <br/>
///     Default implement of <see cref="INotifiableList{T}"/>.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public class NotifiableList<T> : NotifiableListSimpleBase<T>
{
    private readonly List<T> _innerList = [];


    /// <inheritdoc />
    protected sealed override IReadOnlyList<T> View => field ??= _innerList.AsReadOnly();

    /// <inheritdoc />
    protected sealed override T GetItem(int index) => _innerList[index];

    /// <inheritdoc />
    protected sealed override IReadOnlyList<T> GetItems(int index, int count) => [.._innerList[index..(index + count)]];

    /// <inheritdoc />
    protected sealed override void SetItem(int index, T item) => _innerList[index] = item;

    /// <inheritdoc />
    protected sealed override void InsertItem(int index, T item) => _innerList.Insert(index, item);

    /// <inheritdoc />
    protected sealed override void InsertItems(int index, IEnumerable<T> items) => _innerList.InsertRange(index, items);

    /// <inheritdoc />
    protected sealed override void RemoveItem(int index) => _innerList.RemoveAt(index);

    /// <inheritdoc />
    protected sealed override void RemoveItems(int index, int count) => _innerList.RemoveRange(index, count);

    /// <inheritdoc />
    protected sealed override void ClearItems() => _innerList.Clear();

    /// <inheritdoc />
    protected sealed override void SortItems(int index, int count, IComparer<T> comparer) => _innerList.Sort(index, count, comparer);

    /// <inheritdoc />
    protected sealed override void ReverseItems(int index, int count) => _innerList.Reverse(index, count);
}