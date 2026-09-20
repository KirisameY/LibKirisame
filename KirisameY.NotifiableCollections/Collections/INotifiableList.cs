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
    public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated;

    /// <summary>
    ///     与 <see cref="ListUpdated"/> 是<b>同一个</b>事件。
    ///     <br/>
    ///     This is the <b>same</b> event as <see cref="ListUpdated"/>.
    /// </summary>
    /// <remarks>
    ///     订阅它等价于订阅 <see cref="ListUpdated"/>。
    ///     <br/>
    ///     Subscribing to it is equivalent to subscribing to <see cref="ListUpdated"/>.
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
}
