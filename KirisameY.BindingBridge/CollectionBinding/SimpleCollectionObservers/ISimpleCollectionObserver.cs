namespace KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

public interface ISimpleCollectionObserver<in TElement>
{
    void CollectionItemAdded(IEnumerable<TElement> elements);
    void CollectionItemRemoved(IEnumerable<TElement> elements);
    void CollectionItemReplaced(IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements);
    void CollectionReset(IReadOnlyCollection<TElement> collectionView);
}