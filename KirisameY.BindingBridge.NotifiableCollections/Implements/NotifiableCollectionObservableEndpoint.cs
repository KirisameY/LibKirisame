using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class NotifiableCollectionObservableEndpoint<TCollection, TElement>(
    Func<TCollection, IReadOnlyNotifiableCollection<TElement>> collectionGetter
) : ICollectionObservableEndpoint<TCollection, TElement> where TCollection : class
{
    public IReadOnlyCollection<TElement> GetCollectionView(TCollection obj) => collectionGetter.Invoke(obj);

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
        if (list.Count == 0) collectionGetter.Invoke(obj).CollectionUpdated += Handler;
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
        if (removed.Count == 0) collectionGetter.Invoke(obj).CollectionUpdated -= Handler;
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

    private void Handler(object? sender, CollectionUpdateEventArgs<TElement> args)
    {
        ImmutableList<
            (CollectionItemAddedHandler<TElement> addHandler,
            CollectionItemRemovedHandler<TElement> removedHandler,
            CollectionItemReplacedHandler<TElement> replacedHandler,
            CollectionResetHandler<TElement> resetHandler)
        >? list;
        using (_cacheLock.EnterScope())
        {
            if (!_cache.TryGetValue(sender!, out list)) return;
        }

        switch (args)
        {
            case ICollectionItemAddedEventArgs<TElement> added:
            {
                list.ForEach(t => t.addHandler.Invoke(added.AddedItems));
                break;
            }
            case ICollectionItemRemovedEventArgs<TElement> removed:
            {
                list.ForEach(t => t.removedHandler.Invoke(removed.RemovedItems));
                break;
            }
            case ICollectionItemReplacedEventArgs<TElement> replaced:
            {
                list.ForEach(t => t.replacedHandler.Invoke(replaced.OldItems, replaced.NewItems));
                break;
            }
            case ICollectionResetEventArgs<TElement> reset:
            {
                list.ForEach(t => t.resetHandler.Invoke(reset.CollectionView));
                break;
            }
        }
    }
}