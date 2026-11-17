using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可订阅集合变更通知的无重复集合。
///     <br/>
///     A set whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public interface ISetUpdateNotifier<T> : ICollectionUpdateNotifier<T>
{
    /// <summary>
    ///     集合内容发生变化时触发。
    ///     <br/>
    ///     Raised when the contents of the set change.
    /// </summary>
    public event EventHandler<SetUpdateEventArgs<T>> SetUpdated;

    /// <summary>
    ///     与 <see cref="SetUpdated"/> 是<b>同一个</b>事件。
    ///     <br/>
    ///     This is the <b>same</b> event as <see cref="SetUpdated"/>.
    /// </summary>
    /// <remarks>
    ///     订阅它等价于订阅 <see cref="SetUpdated"/>。
    ///     <br/>
    ///     Subscribing to it is equivalent to subscribing to <see cref="SetUpdated"/>.
    /// </remarks>
    event EventHandler<CollectionUpdateEventArgs<T>>? ICollectionUpdateNotifier<T>.CollectionUpdated
    {
        add => SetUpdated += value;
        remove => SetUpdated -= value;
    }
}

/// <summary>
///     只读、但可以订阅变更通知的无重复集合。
///     <br/>
///     A read-only set whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public interface IReadOnlyNotifiableSet<T> : IReadOnlyNotifiableCollection<T>, IReadOnlySet<T>, ISetUpdateNotifier<T>;