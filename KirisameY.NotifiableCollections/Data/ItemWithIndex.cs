namespace KirisameY.NotifiableCollections.Data;

/// <summary>
///     一个元素连同它在列表中的位置，用于在变更通知里把元素和它的原始索引配成对。
///     <br/>
///     An element together with its position in a list, used by change notifications to pair an element with its
///     original index.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the element.
/// </typeparam>
/// <remarks>
///     索引记的是变更<b>之前</b>的口径：变更发生后列表会收缩或增长，用新索引无法还原元素本来的位置。
///     <br/>
///     The index is measured <b>before</b> the change: the list shrinks or grows afterwards, so the new indices
///     could not restore where the element originally sat.
/// </remarks>
public interface IItemWithIndex<out T>
{
    /// <summary>
    ///     该元素在变更前的原始索引。
    ///     <br/>
    ///     The element's original index, measured before the change.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     元素本身。
    ///     <br/>
    ///     The element itself.
    /// </summary>
    public T Item { get; }
}

/// <summary>
///     构造 <see cref="IItemWithIndex{T}"/> 的工厂。
///     <br/>
///     Factory for <see cref="IItemWithIndex{T}"/>.
/// </summary>
public static class ItemWithIndex
{
    /// <summary>
    ///     把元素和它的索引配成一对。
    ///     <br/>
    ///     Pairs an element with its index.
    /// </summary>
    public static IItemWithIndex<T> From<T>(int index, T item) => new ItemWithIndex<T>(index, item);

    /// <summary>
    ///     链式写法，等价于 <see cref="From{T}"/>。
    ///     <br/>
    ///     A fluent form equivalent to <see cref="From{T}"/>.
    /// </summary>
    public static IItemWithIndex<T> WithIndex<T>(this T item, int index) => From(index, item);
}

/// <summary>
///     <see cref="IItemWithIndex{T}"/> 的默认实现。
///     <br/>
///     The default implementation of <see cref="IItemWithIndex{T}"/>.
/// </summary>
/// <param name="Index">
///     该元素在变更前的原始索引。
///     <br/>
///     The element's original index, measured before the change.
/// </param>
/// <param name="Item">
///     元素本身。
///     <br/>
///     The element itself.
/// </param>
public record ItemWithIndex<T>(int Index, T Item) : IItemWithIndex<T>;
