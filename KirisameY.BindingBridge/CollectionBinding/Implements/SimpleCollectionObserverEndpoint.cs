using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class SimpleCollectionObserverEndpoint<TCollection, TElement>(
    Func<TCollection, ISimpleCollectionObserver<TElement>> observerGetter
) : ICollectionObserverEndpoint<TCollection, TElement> where TCollection : class
{
    public void CollectionItemAdded(TCollection obj, IEnumerable<TElement> added)
    {
        var observer = observerGetter.Invoke(obj);
        foreach (var element in added) observer.Add(element);
    }

    public void CollectionItemRemoved(TCollection obj, IEnumerable<TElement> removed)
    {
        var observer = observerGetter.Invoke(obj);
        foreach (var element in removed) observer.Remove(element);
    }

    public void CollectionItemReplaced(TCollection obj, IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements)
    {
        var observer = observerGetter.Invoke(obj);
        foreach (var (old, @new) in oldElements.Zip(newElements))
        {
            observer.Remove(old);
            observer.Add(@new);
        }
    }

    public void CollectionReset(TCollection obj, IReadOnlyCollection<TElement> collectionView)
    {
        var observer = observerGetter.Invoke(obj);
        observer.Clear();
        foreach (var element in collectionView) observer.Add(element);
    }
}