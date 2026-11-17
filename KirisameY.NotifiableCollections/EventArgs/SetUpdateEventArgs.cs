using System.Collections.Frozen;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     无重复集合变更事件参数的公共基类。
///     <br/>
///     Common base class for set change event args.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <param name="setView">
///     触发本次变更的集合视图。
///     <br/>
///     The set view that raised this change.
/// </param>
public abstract class SetUpdateEventArgs<T>(IReadOnlySet<T> setView) : CollectionUpdateEventArgs<T>(setView), ISetUpdateEventArgs<T>
{
    /// <inheritdoc />
    public IReadOnlySet<T> SetView => setView;
}

internal class SetElementAddedEventArgs<T>(IReadOnlySet<T> setView, IReadOnlySet<T> addedElements)
    : SetUpdateEventArgs<T>(setView), ISetElementAddedEventArgs<T>
{
    public IReadOnlySet<T> AddedElements => addedElements;
}

internal class SetElementRemovedEventArgs<T>(IReadOnlySet<T> setView, IReadOnlySet<T> removedElements)
    : SetUpdateEventArgs<T>(setView), ISetElementRemovedEventArgs<T>
{
    public IReadOnlySet<T> RemovedElements => removedElements;
}

internal class SetElementClearedEventArgs<T>(IReadOnlySet<T> setView, IReadOnlySet<T> removedElements)
    : SetElementRemovedEventArgs<T>(setView, removedElements), ISetElementClearedEventArgs<T>;

internal class SetElementRemovedByReplaceEventArgs<T>(IReadOnlySet<T> setView, IReadOnlyDictionary<T, T> replacePairs)
    : SetUpdateEventArgs<T>(setView), ISetElementRemovedByReplaceEventArgs<T>
{
    public IReadOnlyDictionary<T, T> ReplacePairs => replacePairs;
    public IReadOnlySet<T> RemovedElements => field ??= replacePairs.Keys.ToFrozenSet();
}

internal class SetElementReplacedEventArgs<T>(IReadOnlySet<T> setView, IReadOnlyCollection<T> oldItems, IReadOnlyCollection<T> newItems)
    : SetUpdateEventArgs<T>(setView), ISetElementReplacedEventArgs<T>
{
    public IReadOnlyCollection<T> OldItems => oldItems;
    public IReadOnlyCollection<T> NewItems => newItems;

    public IReadOnlyCollection<IItemReplaceInfo<T>> ItemChanges => field ??=
    [
        ..OldItems.Zip(NewItems)
                  .Select(t => ItemReplaceInfo.From(t.First, t.Second))
    ];
}

internal class SetResetEventArgs<T>(IReadOnlySet<T> setView) : SetUpdateEventArgs<T>(setView), ISetResetEventArgs<T>;