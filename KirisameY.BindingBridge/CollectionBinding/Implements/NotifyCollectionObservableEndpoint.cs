using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class NotifyCollectionObservableEndpoint<TCollection, TElement>(
    Func<TCollection, IReadOnlyCollection<TElement>> viewGetter,
    Func<TCollection, INotifyCollectionChanged> notifierGetter
) : ICollectionObservableEndpoint<TCollection, TElement>
    where TCollection : class
{
    public IReadOnlyCollection<TElement> GetCollectionView(TCollection obj) => viewGetter.Invoke(obj);

    public void SubscribeCollectionUpdate(
        TCollection obj,
        CollectionItemAddedHandler<TElement> addHandler,
        CollectionItemRemovedHandler<TElement> removedHandler,
        CollectionItemReplacedHandler<TElement> replacedHandler,
        CollectionResetHandler<TElement> resetHandler
    )
    {
        using var _ = _cacheLock.EnterScope();
        if (!_cache.TryGetValue(obj, out var list)) list = [];
        _cache.AddOrUpdate(obj, list.Add((addHandler, removedHandler, replacedHandler, resetHandler)));
        if (list.Count == 0) notifierGetter.Invoke(obj).CollectionChanged += Handler;
    }

    public void UnsubscribeCollectionUpdate(
        TCollection obj,
        CollectionItemAddedHandler<TElement> addHandler,
        CollectionItemRemovedHandler<TElement> removedHandler,
        CollectionItemReplacedHandler<TElement> replacedHandler,
        CollectionResetHandler<TElement> resetHandler
    )
    {
        using var _ = _cacheLock.EnterScope();
        if (!_cache.TryGetValue(obj, out var list)) return;
        var removed = list.Remove((addHandler, removedHandler, replacedHandler, resetHandler));
        _cache.AddOrUpdate(obj, removed);
        if (removed.Count == 0) notifierGetter.Invoke(obj).CollectionChanged -= Handler;
    }


    private readonly ConditionalWeakTable<
        object, ImmutableList<
            (CollectionItemAddedHandler<TElement> addHandler,
            CollectionItemRemovedHandler<TElement> removedHandler,
            CollectionItemReplacedHandler<TElement> replacedHandler,
            CollectionResetHandler<TElement> resetHandler)
        >
    > _cache = [];

    private readonly Lock _cacheLock = new();

    private void Handler(object? sender, NotifyCollectionChangedEventArgs args)
    {
        var view = viewGetter.Invoke((TCollection)sender!);

        ImmutableList<
            (CollectionItemAddedHandler<TElement> addHandler,
            CollectionItemRemovedHandler<TElement> removedHandler,
            CollectionItemReplacedHandler<TElement> replacedHandler,
            CollectionResetHandler<TElement> resetHandler)
        >? list;
        using (_cacheLock.EnterScope())
        {
            if (!_cache.TryGetValue(sender, out list)) return;
        }

        switch (args)
        {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems }:
            {
                list.ForEach(t => t.addHandler.Invoke(newItems.Cast<TElement>()));
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: { } removedItems }:
            {
                list.ForEach(t => t.removedHandler.Invoke(removedItems.Cast<TElement>()));
                break;
            }
            case { Action: NotifyCollectionChangedAction.Replace, OldItems: { } oldItems, NewItems: { } newItems }:
            {
                list.ForEach(t => t.replacedHandler.Invoke(
                                 oldItems.Cast<TElement>(), newItems.Cast<TElement>()));
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