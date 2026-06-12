using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

#region Interfaces

[PublicAPI]
public interface ICollectionUpdateEventArgs<out T>
{
    [PublicAPI] public IReadOnlyCollection<T> CollectionView { get; }
}

[PublicAPI]
public interface ICollectionItemAddedEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyCollection<T> AddedItems { get; }
}

[PublicAPI]
public interface ICollectionItemRemovedEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyCollection<T> RemovedItems { get; }
}

[PublicAPI]
public interface ICollectionItemClearedEventArgs<out T> : ICollectionItemRemovedEventArgs<T>;

[PublicAPI]
public interface ICollectionItemReplacedEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyCollection<T> OldItems { get; }
    [PublicAPI] public IReadOnlyCollection<T> NewItems { get; }
    [PublicAPI] public IReadOnlyCollection<IItemReplaceInfo<T>> ItemChanges { get; }
}

#endregion

#region Classes

public abstract class CollectionUpdateEventArgs<T>(IReadOnlyCollection<T> collectionView) : System.EventArgs, ICollectionUpdateEventArgs<T>
{
    public IReadOnlyCollection<T> CollectionView => collectionView;
}

// internal class CollectionItemAddedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> addedItems)
//     : CollectionUpdateEventArgs<T>(collectionView), ICollectionItemAddedEventArgs<T>
// {
//     public IReadOnlyCollection<T> AddedItems => addedItems;
// }
//
// internal class CollectionItemRemovedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> removedItems)
//     : CollectionUpdateEventArgs<T>(collectionView), ICollectionItemRemovedEventArgs<T>
// {
//     public IReadOnlyCollection<T> RemovedItems => removedItems;
// }
//
// internal class CollectionItemClearedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> removedItems)
//     : CollectionItemRemovedEventArgs<T>(collectionView, removedItems), ICollectionItemClearedEventArgs<T>;
//
// internal class CollectionItemReplacedEventArgs<T>(IReadOnlyCollection<T> collectionView, IReadOnlyCollection<T> oldItems, IReadOnlyCollection<T> newItems)
//     : CollectionUpdateEventArgs<T>(collectionView), ICollectionItemReplacedEventArgs<T>
// {
//     public IReadOnlyCollection<T> OldItems => oldItems;
//     public IReadOnlyCollection<T> NewItems => newItems;
//     public IReadOnlyCollection<(T old, T @new)> ItemChanges => field ??= [..OldItems.Zip(NewItems)];
// }

#endregion