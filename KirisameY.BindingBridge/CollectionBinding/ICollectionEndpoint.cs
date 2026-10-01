namespace KirisameY.BindingBridge.CollectionBinding;

public interface ICollectionObservableEndpointBase<in TObj, out TElement> where TObj : class;

public interface ICollectionObserverEndpointBase<in TObj, in TElement> where TObj : class;

public interface ICollectionObservableEndpoint<in TObj, out TElement> : ICollectionObservableEndpointBase<TObj, TElement> where TObj : class
{
    IReadOnlyCollection<TElement> GetCollectionView(TObj obj);

    void SubscribeCollectionUpdate(
        TObj obj,
        CollectionItemAddedHandler<TElement> addHandler,
        CollectionItemRemovedHandler<TElement> removedHandler,
        CollectionItemReplacedHandler<TElement> replacedHandler,
        CollectionResetHandler<TElement> resetHandler
    );

    void UnsubscribeCollectionUpdate(
        TObj obj,
        CollectionItemAddedHandler<TElement> addHandler,
        CollectionItemRemovedHandler<TElement> removedHandler,
        CollectionItemReplacedHandler<TElement> replacedHandler,
        CollectionResetHandler<TElement> resetHandler
    );
}

public interface ICollectionObserverEndpoint<in TObj, in TElement> : ICollectionObserverEndpointBase<TObj, TElement> where TObj : class
{
    void CollectionItemAdded(TObj obj, IEnumerable<TElement> added);
    void CollectionItemRemoved(TObj obj, IEnumerable<TElement> removed);
    void CollectionItemReplaced(TObj obj, IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements);
    void CollectionReset(TObj obj, IReadOnlyCollection<TElement> collectionView);
}

public interface IListObservableEndpoint<in TObj, out TItem> : ICollectionObservableEndpointBase<TObj, TItem> where TObj : class
{
    IReadOnlyList<TItem> GetListView(TObj obj);

    void SubscribeListUpdate(
        TObj obj,
        ListItemAddedHandler<TItem> addHandler,
        ListItemRemovedHandler<TItem> removedHandler,
        ListItemReplacedHandler<TItem> replacedHandler,
        ListItemMovedHandler<TItem> movedHandler,
        ListResetHandler<TItem> resetHandler
    );

    void UnsubscribeListUpdate(
        TObj obj,
        ListItemAddedHandler<TItem> addHandler,
        ListItemRemovedHandler<TItem> removedHandler,
        ListItemReplacedHandler<TItem> replacedHandler,
        ListItemMovedHandler<TItem> movedHandler,
        ListResetHandler<TItem> resetHandler
    );
}

public interface IListObserverEndpoint<in TObj, in TItem> : ICollectionObserverEndpointBase<TObj, TItem> where TObj : class
{
    void ListItemAdded(TObj obj, IEnumerable<TItem> added, IEnumerable<int>? indexes);
    void ListItemRemoved(TObj obj, IEnumerable<TItem> removed, IEnumerable<int>? indexes);
    void ListItemReplaced(TObj obj, IEnumerable<TItem> oldItems, IEnumerable<TItem> newItems, IEnumerable<int>? indexes);
    void ListItemMoved(TObj obj, IEnumerable<TItem> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes);
    void ListReset(TObj obj, IReadOnlyList<TItem> listView);
}