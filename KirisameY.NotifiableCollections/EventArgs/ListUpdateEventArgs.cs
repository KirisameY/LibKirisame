using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

#region Interfaces

[PublicAPI]
public interface IListUpdateEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyList<T> ListView { get; }
}

[PublicAPI]
public interface IListItemAddedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemAddedEventArgs<T>
{
    [PublicAPI] public new IReadOnlyList<T> AddedItems { get; }
    [PublicAPI] public int StartIndex { get; }

    IReadOnlyCollection<T> ICollectionItemAddedEventArgs<T>.AddedItems => AddedItems;
}

[PublicAPI]
public interface IListItemRemovedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemRemovedEventArgs<T>
{
    [PublicAPI] public new IReadOnlyList<T> RemovedItems { get; }
    [PublicAPI] public IReadOnlyList<int> Indexes { get; }
    [PublicAPI] public IReadOnlyList<IItemWithIndex<T>> RemovedItemsWithIndex { get; }

    IReadOnlyCollection<T> ICollectionItemRemovedEventArgs<T>.RemovedItems => RemovedItems;
}

[PublicAPI]
public interface IListItemClearedEventArgs<out T> : IListItemRemovedEventArgs<T>;

[PublicAPI]
public interface IListItemReplacedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemReplacedEventArgs<T>
{
    [PublicAPI] public new IReadOnlyList<T> OldItems { get; }
    [PublicAPI] public new IReadOnlyList<T> NewItems { get; }
    [PublicAPI] public IReadOnlyList<int> Indexes { get; }
    [PublicAPI] public new IReadOnlyList<IListItemReplaceInfo<T>> ItemChanges { get; }

    IReadOnlyCollection<T> ICollectionItemReplacedEventArgs<T>.OldItems => OldItems;
    IReadOnlyCollection<T> ICollectionItemReplacedEventArgs<T>.NewItems => NewItems;
    IReadOnlyCollection<IItemReplaceInfo<T>> ICollectionItemReplacedEventArgs<T>.ItemChanges => ItemChanges;
}

[PublicAPI]
public interface IListSortedEventArgs<out T> : IListUpdateEventArgs<T> { }

#endregion

public abstract class ListUpdateEventArgs<T>(IReadOnlyList<T> listView) : CollectionUpdateEventArgs<T>(listView), IListUpdateEventArgs<T>
{
    public IReadOnlyList<T> ListView => listView;
}

internal class ListItemAddedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> addedItems, int startIndex)
    : ListUpdateEventArgs<T>(listView), IListItemAddedEventArgs<T>
{
    public IReadOnlyList<T> AddedItems => addedItems;
    public int StartIndex => startIndex;
}

internal class ListItemRemovedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> removedItems, IReadOnlyList<int> indexes)
    : ListUpdateEventArgs<T>(listView), IListItemRemovedEventArgs<T>
{
    public IReadOnlyList<T> RemovedItems => removedItems;
    public IReadOnlyList<int> Indexes => indexes;
    public IReadOnlyList<IItemWithIndex<T>> RemovedItemsWithIndex => field ??= [..Indexes.Zip(RemovedItems, ItemWithIndex.From)];
}

internal class ListItemClearedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> removedItems, IReadOnlyList<int> indexes)
    : ListItemRemovedEventArgs<T>(listView, removedItems, indexes), IListItemClearedEventArgs<T>;

internal class ListItemReplacedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> oldItems, IReadOnlyList<T> newItems, IReadOnlyList<int> indexes)
    : ListUpdateEventArgs<T>(listView), IListItemReplacedEventArgs<T>
{
    public IReadOnlyList<T> OldItems => oldItems;
    public IReadOnlyList<T> NewItems => newItems;
    public IReadOnlyList<int> Indexes => indexes;

    public IReadOnlyList<IListItemReplaceInfo<T>> ItemChanges => field ??=
    [
        ..Indexes.Zip(OldItems, NewItems)
                 .Select(t => ItemReplaceInfo.From(t.First, t.Second, t.Third))
    ];
}

internal class ListSortedEventArgs<T>(IReadOnlyList<T> listView) : ListUpdateEventArgs<T>(listView);