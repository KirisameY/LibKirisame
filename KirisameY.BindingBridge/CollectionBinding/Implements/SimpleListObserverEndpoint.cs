using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class SimpleListObserverEndpoint<TList, TElement>(
    Func<TList, ISimpleListObserver<TElement>> observerGetter
) : IListObserverEndpoint<TList, TElement> where TList : class
{
    public void ListItemAdded(TList obj, IEnumerable<TElement> added, IEnumerable<int>? indexes) =>
        observerGetter.Invoke(obj).ListItemAdded(added, indexes);

    public void ListItemRemoved(TList obj, IEnumerable<TElement> removed, IEnumerable<int>? indexes) =>
        observerGetter.Invoke(obj).ListItemRemoved(removed, indexes);

    public void ListItemReplaced(TList obj, IEnumerable<TElement> oldItems, IEnumerable<TElement> newItems, IEnumerable<int>? indexes) =>
        observerGetter.Invoke(obj).ListItemReplaced(oldItems, newItems, indexes);

    public void ListItemMoved(TList obj, IEnumerable<TElement> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes) =>
        observerGetter.Invoke(obj).ListItemMoved(items, oldIndexes, newIndexes);

    public void ListReset(TList obj, IReadOnlyList<TElement> listView) =>
        observerGetter.Invoke(obj).ListReset(listView);
}