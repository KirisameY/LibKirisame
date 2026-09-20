using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     列表变更事件参数的公共基类。
///     <br/>
///     Common base class for list change event args.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <param name="listView">
///     触发本次变更的列表视图。
///     <br/>
///     The list view that raised this change.
/// </param>
public abstract class ListUpdateEventArgs<T>(IReadOnlyList<T> listView) : CollectionUpdateEventArgs<T>(listView), IListUpdateEventArgs<T>
{
    /// <inheritdoc cref="IListUpdateEventArgs{T}.ListView"/>
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

internal class ListSortedEventArgs<T>(IReadOnlyList<T> listView) : ListUpdateEventArgs<T>(listView), IListSortedEventArgs<T>;