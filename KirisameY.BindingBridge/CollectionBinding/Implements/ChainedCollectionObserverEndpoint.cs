namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class ChainedCollectionObserverEndpoint<TObj, TCollection, TElement>(
    ICollectionObserverEndpoint<TCollection, TElement> endpoint,
    Func<TObj, TCollection> collectionGetter
) : ICollectionObserverEndpoint<TObj, TElement>
    where TObj : class where TCollection : class
{
    public void CollectionItemAdded(TObj obj, IEnumerable<TElement> added) =>
        endpoint.CollectionItemAdded(collectionGetter.Invoke(obj), added);

    public void CollectionItemRemoved(TObj obj, IEnumerable<TElement> removed) =>
        endpoint.CollectionItemRemoved(collectionGetter.Invoke(obj), removed);

    public void CollectionItemReplaced(TObj obj, IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements) =>
        endpoint.CollectionItemReplaced(collectionGetter.Invoke(obj), oldElements, newElements);

    public void CollectionReset(TObj obj, IReadOnlyCollection<TElement> collectionView) =>
        endpoint.CollectionReset(collectionGetter.Invoke(obj), collectionView);
}