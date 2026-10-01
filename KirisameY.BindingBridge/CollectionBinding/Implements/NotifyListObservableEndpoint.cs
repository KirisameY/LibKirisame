using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class NotifyListObservableEndpoint<TList, TItem>(
    Func<TList, IReadOnlyList<TItem>> viewGetter,
    Func<TList, INotifyCollectionChanged> notifierGetter
) : IListObservableEndpoint<TList, TItem>
    where TList : class
{
    public IReadOnlyList<TItem> GetListView(TList obj) => viewGetter.Invoke(obj);

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
        if (list.Count == 0) notifierGetter.Invoke(obj).CollectionChanged += Handler;
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
        if (removed.Count == 0) notifierGetter.Invoke(obj).CollectionChanged -= Handler;
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

    private void Handler(object? sender, NotifyCollectionChangedEventArgs args)
    {
        var view = viewGetter.Invoke((TList)sender!);

        ImmutableList<
            (ListItemAddedHandler<TItem> addHandler,
            ListItemRemovedHandler<TItem> removedHandler,
            ListItemReplacedHandler<TItem> replacedHandler,
            ListItemMovedHandler<TItem> movedHandler,
            ListResetHandler<TItem> resetHandler)
        >? list;
        using (_cacheLock.EnterScope())
        {
            if (!_cache.TryGetValue(sender, out list)) return;
        }

        switch (args)
        {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems, NewStartingIndex: var index }:
            {
                list.ForEach(t => t.addHandler.Invoke(
                                 newItems.Cast<TItem>(),
                                 index < 0 ? null : Enumerable.Repeat(index, newItems.Count)
                             ));
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: { } removedItems, OldStartingIndex: var index }:
            {
                list.ForEach(t => t.removedHandler.Invoke(
                                 removedItems.Cast<TItem>(),
                                 index < 0 ? null : Enumerable.Repeat(index, removedItems.Count)
                             ));
                break;
            }
            case { Action: NotifyCollectionChangedAction.Replace, OldItems: { } oldItems, NewItems: { } newItems, OldStartingIndex: var index }:
            {
                list.ForEach(t => t.replacedHandler.Invoke(
                                 oldItems.Cast<TItem>(), newItems.Cast<TItem>(),
                                 index < 0 ? null : Enumerable.Repeat(index, oldItems.Count)
                             ));
                break;
            }
            case { Action: NotifyCollectionChangedAction.Move, OldItems: [{ } item], OldStartingIndex: var oldIndex, NewStartingIndex: var newIndex }:
            {
                list.ForEach(t => t.movedHandler.Invoke(
                                 [(TItem)item], [oldIndex], [newIndex]
                             ));
                break;
            }
            case { Action: NotifyCollectionChangedAction.Reset }:
            {
                list.ForEach(t => t.resetHandler.Invoke(view));
                break;
            }
        }
    }
}