using KirisameY.GenericUtils;

namespace KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

public interface ISimpleListObserver<in TItem>
{
    int Find(TItem item);
    void Add(TItem item);
    void Insert(int index, TItem item);
    void RemoveAt(int index);
    void Replace(int index, TItem newItem);
    void Clear();
}