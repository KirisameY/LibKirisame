using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可订阅列表变更通知的列表。
///     <br/>
///     A list whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public interface IListUpdateNotifier<T> : ICollectionUpdateNotifier<T>
{
    /// <summary>
    ///     列表内容发生变化时触发。
    ///     <br/>
    ///     Raised when the contents of the list change.
    /// </summary>
    /// <remarks>
    ///     与集合级的 <see cref="ICollectionUpdateNotifier{T}.CollectionUpdated"/> 相比，这里的事件参数额外携带索引信息
    ///     （具体见 <see cref="ListUpdateEventArgs{T}"/> 体系）。
    ///     <br/>
    ///     Compared with the collection-level <see cref="ICollectionUpdateNotifier{T}.CollectionUpdated"/>,
    ///     the event args here additionally carry index information (see the
    ///     <see cref="ListUpdateEventArgs{T}"/> family).
    /// </remarks>
    public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated;

    /// <summary>
    ///     与 <see cref="ListUpdated"/> 是<b>同一个</b>事件。
    ///     <br/>
    ///     This is the <b>same</b> event as <see cref="ListUpdated"/>.
    /// </summary>
    /// <remarks>
    ///     显式实现而不是另开一个事件，是为了让列表被当作
    ///     <see cref="ICollectionUpdateNotifier{T}"/> 使用时也能收到通知；
    ///     订阅它等价于订阅 <see cref="ListUpdated"/>。
    ///     <br/>
    ///     It is implemented explicitly rather than declared as a separate event so that the list still
    ///     delivers notifications when it is used as an <see cref="ICollectionUpdateNotifier{T}"/>;
    ///     subscribing to it is equivalent to subscribing to <see cref="ListUpdated"/>.
    /// </remarks>
    event EventHandler<CollectionUpdateEventArgs<T>>? ICollectionUpdateNotifier<T>.CollectionUpdated
    {
        add => ListUpdated += value;
        remove => ListUpdated -= value;
    }
}

/// <summary>
///     只读、但可以订阅变更通知的列表。
///     <br/>
///     A read-only list whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public interface IReadOnlyNotifiableList<T> : IReadOnlyNotifiableCollection<T>, IReadOnlyList<T>, IListUpdateNotifier<T>;

/// <summary>
///     可读写、且可以订阅变更通知的列表。
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
    /// <inheritdoc cref="INotifiableCollection{T}.Count"/>
    public new int Count { get; }

    /// <summary>
    ///     按索引读写元素。
    ///     <br/>
    ///     Reads or writes an element by its index.
    /// </summary>
    /// <remarks>
    ///     set 会触发一次「替换」变更；用 <c>new</c> 把 <see cref="IList{T}"/> 与
    ///     <see cref="IReadOnlyList{T}"/> 两份同名索引器收敛成一个。
    ///     <br/>
    ///     Setting a value raises a single "replace" change. <c>new</c> is used to collapse the two
    ///     identically named indexers inherited from <see cref="IList{T}"/> and
    ///     <see cref="IReadOnlyList{T}"/> into a single one.
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
}
