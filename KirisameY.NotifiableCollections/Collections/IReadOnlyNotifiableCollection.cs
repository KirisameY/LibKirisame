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
    public event EventHandler<CollectionUpdateEventArgs<T>> CollectionUpdated;
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