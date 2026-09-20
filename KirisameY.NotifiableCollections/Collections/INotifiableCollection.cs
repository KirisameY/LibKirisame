using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可订阅变更通知的集合。
///     <br/>
///     A collection whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
public interface ICollectionUpdateNotifier<T>
{
    /// <summary>
    ///     集合内容发生变化时触发。
    ///     <br/>
    ///     Raised when the contents of the collection change.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         参数声明为基类型 <see cref="CollectionUpdateEventArgs{T}"/>，实际类型标识变更的种类
    ///         （添加 / 移除 / 清空 / 替换），处理时通常需要模式匹配到对应的
    ///         <c>ICollectionItem***EventArgs&lt;T&gt;</c>。
    ///         <br/>
    ///         事件在集合改完之后才触发，所以 <see cref="ICollectionUpdateEventArgs{T}.CollectionView"/>
    ///         反映的是变更<b>后</b>的状态。
    ///     </para>
    ///     <para>
    ///         The parameter is declared as the base type <see cref="CollectionUpdateEventArgs{T}"/>; its runtime
    ///         type identifies the kind of change (add / remove / clear / replace), so handlers usually need to
    ///         pattern-match it to the corresponding <c>ICollectionItem***EventArgs&lt;T&gt;</c>.
    ///         <br/>
    ///         The event is raised after the collection has been modified, so
    ///         <see cref="ICollectionUpdateEventArgs{T}.CollectionView"/> reflects the state <b>after</b> the change.
    ///     </para>
    /// </remarks>
    public event EventHandler<CollectionUpdateEventArgs<T>>? CollectionUpdated;
}

/// <summary>
///     只读、但可以订阅变更通知的集合。
///     <br/>
///     A read-only collection whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
public interface IReadOnlyNotifiableCollection<T> : IReadOnlyCollection<T>, ICollectionUpdateNotifier<T>;

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
    /// <remarks>
    ///     这里用 <c>new</c> 把 <see cref="ICollection{T}"/> 与 <see cref="IReadOnlyCollection{T}"/>
    ///     两份同名成员收敛成一个，下面再用显式实现把两者都桥接过来。
    ///     <br/>
    ///     <c>new</c> is used to collapse the two identically named members inherited from
    ///     <see cref="ICollection{T}"/> and <see cref="IReadOnlyCollection{T}"/> into a single one;
    ///     the explicit implementations below bridge both of them back.
    /// </remarks>
    public new int Count { get; }

    int ICollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;
}
