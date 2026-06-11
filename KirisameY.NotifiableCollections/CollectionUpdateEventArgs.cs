using System.Collections.Immutable;

using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections;

public abstract class CollectionUpdateEventArgs<T>() : EventArgs { }

public class CollectionItemAddedEventArgs<T>(IReadOnlyCollection<T> addedItems) : CollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyCollection<T> AddedItems => addedItems;
}

public class CollectionItemRemovedEventArgs<T>(IReadOnlyCollection<T> removedItems, bool cleared) : CollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyCollection<T> RemovedItems => removedItems;
    [PublicAPI] public bool Cleared => false;
}

public class CollectionItemReplacedEventArgs<T>(IReadOnlyCollection<T> oldItems, IReadOnlyCollection<T> newItems) : CollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyCollection<T> OldItems => oldItems;
    [PublicAPI] public IReadOnlyCollection<T> NewItems => newItems;
    [PublicAPI] public IReadOnlyCollection<(T old, T @new)> ItemChanges => field ??= [..OldItems.Zip(NewItems)];
}

public class ListItemAddedEventArgs<T>(IReadOnlyList<T> addedItems, int startIndex) : CollectionItemAddedEventArgs<T>(addedItems)
{
    [PublicAPI] public new IReadOnlyList<T> AddedItems => addedItems;
    [PublicAPI] public int StartIndex => startIndex;
}

public class ListItemRemovedEventArgs<T>(IReadOnlyList<T> removedItems, IReadOnlyList<int> indexes, bool cleared) : CollectionItemRemovedEventArgs<T>(removedItems, cleared)
{
    [PublicAPI] public new IReadOnlyList<T> RemovedItems => removedItems;
    [PublicAPI] public IReadOnlyList<int> Indexes => indexes;
    [PublicAPI] public IReadOnlyList<(int index, T item)> RemovedItemsWithIndex => field ??= [..Indexes.Zip(RemovedItems)];
}

public class ListItemReplacedEventArgs<T>(IReadOnlyList<T> oldItems, IReadOnlyList<T> newItems, IReadOnlyList<int> indexes) : CollectionItemReplacedEventArgs<T>(oldItems, newItems)
{
    [PublicAPI] public new IReadOnlyList<T> OldItems => oldItems;
    [PublicAPI] public new IReadOnlyList<T> NewItems => newItems;
    [PublicAPI] public IReadOnlyList<int> Indexes => indexes;

    [PublicAPI] public new IReadOnlyList<(int index, T old, T @new)> ItemChanges => field ??= [..Indexes.Zip(OldItems, NewItems)];
}

public class ListSortedEventArgs<T>(IReadOnlyList<T> newListView) : CollectionUpdateEventArgs<T>
{
    [PublicAPI] public IReadOnlyList<T> NewListView => newListView;
}