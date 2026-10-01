using KirisameY.Relinq.Extensions;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class ModifiableCollectionObservableEndpoint<TCollection, TElement>(
    Func<TCollection, ICollection<TElement>> collectionGetter
) : ICollectionObserverEndpoint<TCollection, TElement>
    where TCollection : class
{
    public void CollectionItemAdded(TCollection obj, IEnumerable<TElement> added)
    {
        var collection = collectionGetter.Invoke(obj);
        added.ForEach(collection.Add);
    }

    public void CollectionItemRemoved(TCollection obj, IEnumerable<TElement> removed)
    {
        var collection = collectionGetter.Invoke(obj);
        removed.ForEach(e => collection.Remove(e));
    }

    public void CollectionItemReplaced(TCollection obj, IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements)
    {
        var collection = collectionGetter.Invoke(obj);
        foreach (var (old, @new) in oldElements.Zip(newElements))
        {
            collection.Remove(old);
            collection.Add(@new);
        }
    }

    public void CollectionReset(TCollection obj, IReadOnlyCollection<TElement> collectionView)
    {
        var collection = collectionGetter.Invoke(obj);
        collection.Clear();
        collectionView.ForEach(collection.Add);
    }
}