using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     集合变更事件参数的公共基类。
///     <br/>
///     Common base class for collection change event args.
/// </summary>
/// <typeparam name="T">
///     集合元素类型。
///     <br/>
///     The type of the collection's elements.
/// </typeparam>
/// <param name="collectionView">
///     触发本次变更的集合视图。
///     <br/>
///     The collection view that raised this change.
/// </param>
public abstract class CollectionUpdateEventArgs<T>(IReadOnlyCollection<T> collectionView) : System.EventArgs, ICollectionUpdateEventArgs<T>
{
    /// <inheritdoc cref="ICollectionUpdateEventArgs{T}.CollectionView"/>
    public IReadOnlyCollection<T> CollectionView => collectionView;
}

internal class CollectionItemAddedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> addedItems)
    : CollectionUpdateEventArgs<T>(collectionView), ICollectionItemAddedEventArgs<T>
{
    public IReadOnlyCollection<T> AddedItems => addedItems;
}

internal class CollectionItemRemovedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> removedItems)
    : CollectionUpdateEventArgs<T>(collectionView), ICollectionItemRemovedEventArgs<T>
{
    public IReadOnlyCollection<T> RemovedItems => removedItems;
}

internal class CollectionItemClearedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> removedItems)
    : CollectionItemRemovedEventArgs<T>(collectionView, removedItems), ICollectionItemClearedEventArgs<T>;

internal class CollectionItemReplacedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> oldItems, IReadOnlyCollection<T> newItems)
    : CollectionUpdateEventArgs<T>(collectionView), ICollectionItemReplacedEventArgs<T>
{
    public IReadOnlyCollection<T> OldItems => oldItems;
    public IReadOnlyCollection<T> NewItems => newItems;

    public IReadOnlyCollection<IItemReplaceInfo<T>> ItemChanges => field ??=
    [
        ..OldItems.Zip(NewItems)
                  .Select(t => ItemReplaceInfo.From(t.First, t.Second))
    ];
}