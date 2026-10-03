namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class ChainedListObserverEndpoint<TObj, TList, TElement>(
    IListObserverEndpoint<TList, TElement> endpoint,
    Func<TObj, TList> listGetter
) : IListObserverEndpoint<TObj, TElement>
    where TObj : class where TList : class
{
    public void ListItemAdded(TObj obj, IEnumerable<TElement> added, IEnumerable<int>? indexes) =>
        endpoint.ListItemAdded(listGetter.Invoke(obj), added, indexes);

    public void ListItemRemoved(TObj obj, IEnumerable<TElement> removed, IEnumerable<int>? indexes) =>
        endpoint.ListItemRemoved(listGetter.Invoke(obj), removed, indexes);

    public void ListItemReplaced(TObj obj, IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements, IEnumerable<int>? indexes) =>
        endpoint.ListItemReplaced(listGetter.Invoke(obj), oldElements, newElements, indexes);

    public void ListItemMoved(TObj obj, IEnumerable<TElement> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes) =>
        endpoint.ListItemMoved(listGetter.Invoke(obj), items, oldIndexes, newIndexes);

    public void ListReset(TObj obj, IReadOnlyList<TElement> listView) =>
        endpoint.ListReset(listGetter.Invoke(obj), listView);
}