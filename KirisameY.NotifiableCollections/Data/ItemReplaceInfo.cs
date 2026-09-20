namespace KirisameY.NotifiableCollections.Data;

/// <summary>
///     一次替换的新旧值对照。
///     <br/>
///     The old and new values of a single replacement.
/// </summary>
/// <typeparam name="T">
///     值的类型。
///     <br/>
///     The type of the values.
/// </typeparam>
public interface IItemReplaceInfo<out T>
{
    /// <summary>
    ///     被替换掉的旧值。
    ///     <br/>
    ///     The old value that was replaced.
    /// </summary>
    public T Old { get; }

    /// <summary>
    ///     替换后的新值。
    ///     <br/>
    ///     The new value it was replaced with.
    /// </summary>
    public T New { get; }
}

/// <summary>
///     列表版的新旧值对照，在上面那对新旧值之外还带上发生替换的索引。
///     <br/>
///     The list-level old/new pair, which additionally carries the index the replacement happened at.
/// </summary>
/// <typeparam name="T">
///     值的类型。
///     <br/>
///     The type of the values.
/// </typeparam>
public interface IListItemReplaceInfo<out T> : IItemReplaceInfo<T>
{
    /// <summary>
    ///     该次替换发生的索引。
    ///     <br/>
    ///     The index the replacement happened at.
    /// </summary>
    public int Index { get; }
}

/// <summary>
///     构造 <see cref="IItemReplaceInfo{T}"/> 的工厂。
///     <br/>
///     Factory for <see cref="IItemReplaceInfo{T}"/>.
/// </summary>
public static class ItemReplaceInfo
{
    /// <summary>
    ///     配一对新旧值。
    ///     <br/>
    ///     Pairs an old value with a new one.
    /// </summary>
    public static IItemReplaceInfo<T> From<T>(T old, T @new) => new ItemReplaceInfo<T>(old, @new);

    /// <summary>
    ///     配一对新旧值，并带上发生替换的索引。
    ///     <br/>
    ///     Pairs an old value with a new one and records the index the replacement happened at.
    /// </summary>
    public static IListItemReplaceInfo<T> From<T>(int index, T old, T @new) => new ListItemReplaceInfo<T>(index, old, @new);
}

/// <summary>
///     <see cref="IItemReplaceInfo{T}"/> 的默认实现。
///     <br/>
///     The default implementation of <see cref="IItemReplaceInfo{T}"/>.
/// </summary>
public class ItemReplaceInfo<T>(T old, T @new) : IItemReplaceInfo<T>
{
    /// <inheritdoc/>
    public T Old { get; } = old;

    /// <inheritdoc/>
    public T New { get; } = @new;
}

/// <summary>
///     <see cref="IListItemReplaceInfo{T}"/> 的默认实现。
///     <br/>
///     The default implementation of <see cref="IListItemReplaceInfo{T}"/>.
/// </summary>
public class ListItemReplaceInfo<T>(int index, T old, T @new) : ItemReplaceInfo<T>(old, @new), IListItemReplaceInfo<T>
{
    /// <inheritdoc/>
    public int Index { get; } = index;
}
