namespace KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

public interface ISimpleListObserver<in TItem>
{
    void ListItemAdded(IEnumerable<TItem> items, IEnumerable<int>? indexes);
    void ListItemRemoved(IEnumerable<TItem> items, IEnumerable<int>? indexes);
    void ListItemReplaced(IEnumerable<TItem> oldItems, IEnumerable<TItem> newItems, IEnumerable<int>? indexes);
    void ListItemMoved(IEnumerable<TItem> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes);
    void ListReset(IReadOnlyList<TItem> listView);
}