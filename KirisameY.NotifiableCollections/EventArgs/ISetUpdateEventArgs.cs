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
    [PublicAPI] IReadOnlySet<T> AddedElements { get; }
    IReadOnlyCollection<T> ICollectionItemAddedEventArgs<T>.AddedItems => AddedElements;
}

[PublicAPI]
public interface ISetElementRemovedEventArgs<T> : ISetUpdateEventArgs<T>, ICollectionItemRemovedEventArgs<T>
{
    [PublicAPI] IReadOnlySet<T> RemovedElements { get; }
    IReadOnlyCollection<T> ICollectionItemRemovedEventArgs<T>.RemovedItems => RemovedElements;
}

[PublicAPI]
public interface ISetElementClearedEventArgs<T> : ISetElementRemovedEventArgs<T>, ICollectionItemClearedEventArgs<T>;

[PublicAPI]
public interface ISetElementRemovedByReplaceEventArgs<T> : ISetElementRemovedEventArgs<T>
{
    [PublicAPI] IReadOnlyDictionary<T, T> ReplacePairs { get; }
}

[PublicAPI]
public interface ISetElementReplacedEventArgs<T> : ISetUpdateEventArgs<T>, ICollectionItemReplacedEventArgs<T>;

[PublicAPI]
public interface ISetResetEventArgs<T> : ISetUpdateEventArgs<T>, ICollectionResetEventArgs<T>;