using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class NotifiableListObservableEndpoint<TList, TItem>(
    Func<TList, IReadOnlyNotifiableList<TItem>> listGetter
) : IListObservableEndpoint<TList, TItem> where TList : class
{
    public IReadOnlyList<TItem> GetListView(TList obj) => listGetter.Invoke(obj);

    public void SubscribeListUpdate(
        TList obj,
        ListItemAddedHandler<TItem> addHandler,
        ListItemRemovedHandler<TItem> removedHandler,
        ListItemReplacedHandler<TItem> replacedHandler,
        ListItemMovedHandler<TItem> movedHandler,
        ListResetHandler<TItem> resetHandler
    )
    {
        using var _ = _cacheLock.EnterScope();
        if (!_cache.TryGetValue(obj, out var list)) list = [];
        _cache.AddOrUpdate(obj, list.Add((addHandler, removedHandler, replacedHandler, movedHandler, resetHandler)));
        if (list.Count == 0) listGetter.Invoke(obj).ListUpdated += Handler;
    }

    public void UnsubscribeListUpdate(
        TList obj,
        ListItemAddedHandler<TItem> addHandler,
        ListItemRemovedHandler<TItem> removedHandler,
        ListItemReplacedHandler<TItem> replacedHandler,
        ListItemMovedHandler<TItem> movedHandler,
        ListResetHandler<TItem> resetHandler
    )
    {
        using var _ = _cacheLock.EnterScope();
        if (!_cache.TryGetValue(obj, out var list)) return;
        var removed = list.Remove((addHandler, removedHandler, replacedHandler, movedHandler, resetHandler));
        _cache.AddOrUpdate(obj, removed);
        if (removed.Count == 0) listGetter.Invoke(obj).ListUpdated -= Handler;
    }


    private readonly ConditionalWeakTable<
        object, ImmutableList<
            (ListItemAddedHandler<TItem> addHandler,
            ListItemRemovedHandler<TItem> removedHandler,
            ListItemReplacedHandler<TItem> replacedHandler,
            ListItemMovedHandler<TItem> movedHandler,
            ListResetHandler<TItem> resetHandler)
        >
    > _cache = [];

    private readonly Lock _cacheLock = new();

    private void Handler(object? sender, ListUpdateEventArgs<TItem> args)
    {
        ImmutableList<
            (ListItemAddedHandler<TItem> addHandler,
            ListItemRemovedHandler<TItem> removedHandler,
            ListItemReplacedHandler<TItem> replacedHandler,
            ListItemMovedHandler<TItem> movedHandler,
            ListResetHandler<TItem> resetHandler)
        >? list;
        using (_cacheLock.EnterScope())
        {
            if (!_cache.TryGetValue(sender!, out list)) return;
        }

        switch (args)
        {
            case IListItemAddedEventArgs<TItem> added:
            {
                list.ForEach(t => t.addHandler.Invoke(added.AddedItems, added.Indexes));
                break;
            }
            case IListItemRemovedEventArgs<TItem> removed:
            {
                list.ForEach(t => t.removedHandler.Invoke(removed.RemovedItems, removed.Indexes));
                break;
            }
            case IListItemReplacedEventArgs<TItem> replaced:
            {
                list.ForEach(t => t.replacedHandler.Invoke(replaced.OldItems, replaced.NewItems, replaced.Indexes));
                break;
            }
            case IListItemMovedEventArgs<TItem> moved:
            {
                list.ForEach(t => t.movedHandler.Invoke(moved.Items, moved.OldIndexes, moved.NewIndexes));
                break;
            }
            case IListSortedEventArgs<TItem> or IListResetEventArgs<TItem>:
            {
                list.ForEach(t => t.resetHandler.Invoke(args.ListView));
                break;
            }
        }
    }
}