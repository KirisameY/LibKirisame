namespace KirisameY.NotifiableCollections.Data;

/// <summary>
///     一次列表项移动的新旧索引对照。
///     <br/>
///     The old and new index of moved list item.
/// </summary>
/// <typeparam name="T">
///     值的类型。
///     <br/>
///     The type of the values.
/// </typeparam>
public interface IListItemMovedInfo<out T>
{
    /// <summary>
    ///     被移动的列表项。
    ///     <br/>
    ///     The moved item.
    /// </summary>
    public T Item { get; }

    /// <summary>
    ///     移动前掉的旧索引。
    ///     <br/>
    ///     The old value that was replaced.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    ///     移动后的新索引。
    ///     <br/>
    ///     The new value it was replaced with.
    /// </summary>
    public int NewIndex { get; }
}

/// <summary>
///     构造 <see cref="IListItemMovedInfo{T}"/> 的工厂。
///     <br/>
///     Factory for <see cref="IListItemMovedInfo{T}"/>.
/// </summary>
public static class ListItemMoveInfo
{
    /// <summary>
    ///     构造一个 <see cref="IListItemMovedInfo{T}"/>。
    ///     <br/>
    ///     Create an <see cref="IListItemMovedInfo{T}"/>.
    /// </summary>
    public static IListItemMovedInfo<T> From<T>(T item, int oldIndex, int newIndex) => new ListItemMovedInfo<T>(item, oldIndex, newIndex);
}

internal class ListItemMovedInfo<T>(T item, int oldIndex, int newIndex) : IListItemMovedInfo<T>
{
    public T Item => item;
    public int OldIndex => oldIndex;
    public int NewIndex => newIndex;
}