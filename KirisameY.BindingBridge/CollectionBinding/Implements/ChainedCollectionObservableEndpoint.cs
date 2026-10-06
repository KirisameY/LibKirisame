using System.Runtime.CompilerServices;

using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class ChainedCollectionObservableEndpoint<TObj, TCollection, TElement>(
    ICollectionObservableEndpoint<TCollection, TElement> endpoint,
    Func<TObj, TCollection> collectionGetter,
    PropertyUpdateNotifyProxy? notify
) : ICollectionObservableEndpoint<TObj, TElement>
    where TObj : class where TCollection : class
{
    public IReadOnlyCollection<TElement> GetCollectionView(TObj obj) => endpoint.GetCollectionView(collectionGetter.Invoke(obj));

    private readonly ConditionalWeakTable<
        TObj, Dictionary<(
            CollectionItemAddedHandler<TElement> addHandler,
            CollectionItemRemovedHandler<TElement> removedHandler,
            CollectionItemReplacedHandler<TElement> replacedHandler,
            CollectionResetHandler<TElement> resetHandler
            ), Action>
    > _updateDicts = [];

    public void SubscribeCollectionUpdate(
        TObj obj,
        CollectionItemAddedHandler<TElement> addHandler,
        CollectionItemRemovedHandler<TElement> removedHandler,
        CollectionItemReplacedHandler<TElement> replacedHandler,
        CollectionResetHandler<TElement> resetHandler
    )
    {
        var collection = collectionGetter.Invoke(obj);
        endpoint.SubscribeCollectionUpdate(collection, addHandler, removedHandler, replacedHandler, resetHandler);

        if (notify is null) return;

        var handler = () =>
        {
            var newCollection = collectionGetter.Invoke(obj);
            endpoint.UnsubscribeCollectionUpdate(collection, addHandler, removedHandler, replacedHandler, resetHandler);
            endpoint.SubscribeCollectionUpdate(newCollection, addHandler, removedHandler, replacedHandler, resetHandler);
            resetHandler.Invoke(endpoint.GetCollectionView(newCollection));
            collection = newCollection;
        };
        if (!_updateDicts.TryGetValue(obj, out var dict))
        {
            dict = [];
            _updateDicts.Add(obj, dict);
        }
        dict.Add((addHandler, removedHandler, replacedHandler, resetHandler), handler);

        notify.Value.SubscribeUpdate(obj, handler);
    }

    public void UnsubscribeCollectionUpdate(
        TObj obj,
        CollectionItemAddedHandler<TElement> addHandler,
        CollectionItemRemovedHandler<TElement> removedHandler,
        CollectionItemReplacedHandler<TElement> replacedHandler,
        CollectionResetHandler<TElement> resetHandler
    )
    {
        var collection = collectionGetter.Invoke(obj);
        endpoint.UnsubscribeCollectionUpdate(collection, addHandler, removedHandler, replacedHandler, resetHandler);

        if (notify is null) return;

        if (
            !_updateDicts.TryGetValue(obj, out var dict) ||
            !dict.Remove((addHandler, removedHandler, replacedHandler, resetHandler), out var handler)
        ) throw new Exception("this exception should not be thrown");

        notify.Value.UnsubscribeUpdate(obj, handler);
    }
}