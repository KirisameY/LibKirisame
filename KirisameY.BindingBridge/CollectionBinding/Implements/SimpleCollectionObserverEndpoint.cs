using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class SimpleCollectionObserverEndpoint<TCollection, TElement>(
    Func<TCollection, ISimpleCollectionObserver<TElement>> observerGetter
) : ICollectionObserverEndpoint<TCollection, TElement> where TCollection : class
{
    public void CollectionItemAdded(TCollection obj, IEnumerable<TElement> added) =>
        observerGetter.Invoke(obj).CollectionItemAdded(added);

    public void CollectionItemRemoved(TCollection obj, IEnumerable<TElement> removed) =>
        observerGetter.Invoke(obj).CollectionItemRemoved(removed);

    public void CollectionItemReplaced(TCollection obj, IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements) =>
        observerGetter.Invoke(obj).CollectionItemReplaced(oldElements, newElements);

    public void CollectionReset(TCollection obj, IReadOnlyCollection<TElement> collectionView) =>
        observerGetter.Invoke(obj).CollectionReset(collectionView);
}