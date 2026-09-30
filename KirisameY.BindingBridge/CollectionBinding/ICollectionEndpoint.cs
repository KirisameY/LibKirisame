namespace KirisameY.BindingBridge.CollectionBinding;

public interface ICollectionEndpoint<in TObj, TElement>;

public interface ICollectionObservableEndpoint<in TObj, TElement> : ICollectionEndpoint<TObj, TElement>
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

public interface ICollectionObserverEndpoint<in TObj, TElement> : ICollectionEndpoint<TObj, TElement>
{
    void CollectionItemAdded(TObj obj, IEnumerable<TElement> added);
    void CollectionItemRemoved(TObj obj, IEnumerable<TElement> removed);
    void CollectionItemReplaced(TObj obj, IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements);
    void CollectionReset(TObj obj, IReadOnlyCollection<TElement> collectionView);
}

public interface IListEndpoint<in TObj, TItem> : ICollectionEndpoint<TObj, TItem>;

public interface IListObservableEndpoint<in TObj, TItem> : IListEndpoint<TObj, TItem>
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

public interface IListObserverEndpoint<in TObj, TItem> : IListEndpoint<TObj, TItem>
{
    void ListItemAdded(TObj obj, IEnumerable<TItem> added, IEnumerable<int>? indexes);
    void ListItemRemoved(TObj obj, IEnumerable<TItem> removed, IEnumerable<int>? indexes);
    void ListItemReplaced(TObj obj, IEnumerable<TItem> oldItems, IEnumerable<TItem> newItems, IEnumerable<int>? indexes);
    void ListItemMoved(TObj obj, IEnumerable<TItem> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes);
    void ListReset(TObj obj, IReadOnlyList<TItem> listView);
}