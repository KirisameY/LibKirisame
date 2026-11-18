using System.Collections.Frozen;
using System.Collections.Immutable;

using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     无重复集合变更事件参数的基接口，只保证能拿到集合视图。
///     <br/>
///     Base interface for set change event args; it only guarantees access to the set view.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
[PublicAPI]
public interface ISetUpdateEventArgs<T> : ICollectionUpdateEventArgs<T>
{
    /// <summary>
    ///     触发本次变更的集合视图，反映变更<b>之后</b>的状态。
    ///     <br/>
    ///     The set view that raised this change, reflecting the state <b>after</b> the change.
    /// </summary>
    [PublicAPI] public IReadOnlySet<T> SetView { get; }
}

/// <summary>
///     一个或多个元素被添加至集合。
///     <br/>
///     One or more elements has added to the set.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the set's elements.
/// </typeparam>
[PublicAPI]
public interface ISetElementAddedEventArgs<T> : ISetUpdateEventArgs<T>, ICollectionItemAddedEventArgs<T>
{
    /// <summary>
    ///     本次被添加的元素。
    ///     <br/>
    ///     The elements added by this change.
    /// </summary>
    [PublicAPI] IReadOnlySet<T> AddedElements { get; }
    IReadOnlyCollection<T> ICollectionItemAddedEventArgs<T>.AddedItems => AddedElements;
}

/// <summary>
///     一个或多个元素被从集合中移除。
///     <br/>
///     One or more elements has added from the set.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the set's elements.
/// </typeparam>
[PublicAPI]
public interface ISetElementRemovedEventArgs<T> : ISetUpdateEventArgs<T>, ICollectionItemRemovedEventArgs<T>
{
    /// <summary>
    ///     本次被移除的元素。
    ///     <br/>
    ///     The elements removed by this change.
    /// </summary>
    [PublicAPI] IReadOnlySet<T> RemovedElements { get; }
    IReadOnlyCollection<T> ICollectionItemRemovedEventArgs<T>.RemovedItems => RemovedElements;
}

/// <summary>
///     集合被清空。
///     <br/>
///     The set was cleared.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the set's elements.
/// </typeparam>
[PublicAPI]
public interface ISetElementClearedEventArgs<T> : ISetElementRemovedEventArgs<T>, ICollectionItemClearedEventArgs<T>;

/// <summary>
///     集合中的一个或多个元素因被替换为集合中已有的项而被移除。
///     <br/>
///     One or more set in the set has been removed by
///     replacing to an element that already exists in the set.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the set's elements.
/// </typeparam>
[PublicAPI]
public interface ISetElementRemovedByReplaceEventArgs<T> : ISetElementRemovedEventArgs<T>
{
    /// <summary>
    ///     被替换掉的元素和尝试将之替换为的新元素的字典。
    ///     <br/>
    ///     A dictionary containing the replaced elements
    ///     and the new elements to be used as their replacement.
    /// </summary>
    [PublicAPI] IReadOnlyDictionary<T, T> ReplacePairs { get; }
}

/// <summary>
///     集合中的一个或多个元素被替换。
///     <br/>
///     One or more elements in the set has been replaced.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the set's elements.
/// </typeparam>
[PublicAPI]
public interface ISetElementReplacedEventArgs<T> : ISetUpdateEventArgs<T>, ICollectionItemReplacedEventArgs<T>;

/// <summary>
///     集合被重置为新状态，需要重新加载。
///     <br/>
///     The set has been reset to a new state and need to be reloaded.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the set's elements.
/// </typeparam>
[PublicAPI]
public interface ISetResetEventArgs<T> : ISetUpdateEventArgs<T>, ICollectionResetEventArgs<T>;