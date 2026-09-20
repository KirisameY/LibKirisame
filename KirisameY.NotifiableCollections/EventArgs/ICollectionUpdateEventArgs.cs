using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     集合变更事件参数的基接口，只保证能拿到集合视图。
///     <br/>
///     Base interface for collection change event args; it only guarantees access to the collection view.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
/// <remarks>
///     这里描述的是<b>无序</b>集合视角：变更项一律用 <see cref="IReadOnlyCollection{T}"/> 表示，
///     <b>不保证任何顺序</b>。需要索引 / 顺序信息请用列表版的
///     <see cref="IListItemRemovedEventArgs{T}"/> 等接口。
///     <br/>
///     This describes an <b>unordered</b> collection: change items are always exposed as
///     <see cref="IReadOnlyCollection{T}"/>, with <b>no ordering guarantee</b>. Use the list-level interfaces
///     such as <see cref="IListItemRemovedEventArgs{T}"/> when index / ordering information is needed.
/// </remarks>
[PublicAPI]
public interface ICollectionUpdateEventArgs<out T>
{
    /// <summary>
    ///     触发本次变更的集合视图，反映变更<b>之后</b>的状态。
    ///     <br/>
    ///     The collection view that raised this change, reflecting the state <b>after</b> the change.
    /// </summary>
    /// <remarks>
    ///     是<b>活视图</b>而非快照：集合之后再变，这里读到的也会跟着变。
    ///     需要留档请自行拷贝一份。
    ///     <br/>
    ///     This is a <b>live view</b>, not a snapshot: later changes to the collection are visible here too.
    ///     Copy it yourself if you need to keep a record.
    /// </remarks>
    [PublicAPI] public IReadOnlyCollection<T> CollectionView { get; }
}

/// <summary>
///     一个或多个元素被添加至集合。
///     <br/>
///     One or more elements has added to the collection.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
[PublicAPI]
public interface ICollectionItemAddedEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    /// <summary>
    ///     本次被添加的元素。
    ///     <br/>
    ///     The elements added by this change.
    /// </summary>
    /// <remarks>
    ///     无序集合，不保证顺序。
    ///     <br/>
    ///     Unordered collection, so no ordering is guaranteed.
    /// </remarks>
    [PublicAPI] public IReadOnlyCollection<T> AddedItems { get; }
}

/// <summary>
///     一个或多个元素被从集合中移除。
///     <br/>
///     One or more elements has added from the collection.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
[PublicAPI]
public interface ICollectionItemRemovedEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    /// <summary>
    ///     本次被移除的元素。
    ///     <br/>
    ///     The elements removed by this change.
    /// </summary>
    /// <remarks>
    ///     无序集合，不保证顺序。
    ///     <br/>
    ///     Unordered collection, so no ordering is guaranteed.
    /// </remarks>
    [PublicAPI] public IReadOnlyCollection<T> RemovedItems { get; }
}

/// <summary>
///     集合被清空。
///     <br/>
///     The collection was cleared.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
/// <remarks>
///     是「移除」的特例：<see cref="ICollectionItemRemovedEventArgs{T}.RemovedItems"/> 即清空前的全部元素。
///     <br/>
///     A special case of "remove": <see cref="ICollectionItemRemovedEventArgs{T}.RemovedItems"/> holds every
///     element the collection contained before being cleared.
/// </remarks>
[PublicAPI]
public interface ICollectionItemClearedEventArgs<out T> : ICollectionItemRemovedEventArgs<T>;

/// <summary>
///     集合中的一个或多个元素被替换。
///     <br/>
///     One or more elements in the collection has been replaced.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
[PublicAPI]
public interface ICollectionItemReplacedEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    /// <summary>
    ///     被替换掉的旧元素。
    ///     <br/>
    ///     The old elements that were replaced.
    /// </summary>
    [PublicAPI] public IReadOnlyCollection<T> OldItems { get; }

    /// <summary>
    ///     替换后的新元素，与 <see cref="OldItems"/> <b>逐位对应</b>。
    ///     <br/>
    ///     The new elements, <b>positionally aligned</b> with <see cref="OldItems"/>.
    /// </summary>
    [PublicAPI] public IReadOnlyCollection<T> NewItems { get; }

    /// <summary>
    ///     <see cref="OldItems"/> 与 <see cref="NewItems"/> 按位配对（Zip）的结果。
    ///     <br/>
    ///     The result of pairing (zipping) <see cref="OldItems"/> with <see cref="NewItems"/> position by position.
    /// </summary>
    /// <remarks>
    ///     依赖上面两者逐位对齐的约定，需要新旧对照时优先用这个属性。
    ///     <br/>
    ///     Relies on the positional alignment of the two properties above; prefer this property when you need
    ///     old/new pairs.
    /// </remarks>
    [PublicAPI] public IReadOnlyCollection<IItemReplaceInfo<T>> ItemChanges { get; }
}