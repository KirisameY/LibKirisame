namespace KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

public interface ISimpleCollectionObserver<in TElement>
{
    void Add(TElement element);
    void Remove(TElement element);
    void Clear();
}