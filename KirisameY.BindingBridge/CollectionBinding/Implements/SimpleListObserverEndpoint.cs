using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class SimpleListObserverEndpoint<TList, TItem>(
    Func<TList, ISimpleListObserver<TItem>> observerGetter
) : IListObserverEndpoint<TList, TItem> where TList : class
{
    public void ListItemAdded(TList obj, IEnumerable<TItem> added, IEnumerable<int>? indexes)
    {
        var observer = observerGetter.Invoke(obj);
        if (indexes is null)
        {
            foreach (var item in added) observer.Add(item);
        }
        else
        {
            foreach (var (i, item) in indexes.Zip(added)) observer.Insert(i, item);
        }
    }

    public void ListItemRemoved(TList obj, IEnumerable<TItem> removed, IEnumerable<int>? indexes)
    {
        var observer = observerGetter.Invoke(obj);
        if (indexes is not null)
        {
            foreach (var i in indexes.Reverse()) observer.RemoveAt(i);
        }
        else
        {
            foreach (var item in removed.Reverse())
            {
                var i = observer.Find(item);
                if (i >= 0) observer.RemoveAt(i);
            }
        }
    }

    public void ListItemReplaced(TList obj, IEnumerable<TItem> oldItems, IEnumerable<TItem> newItems, IEnumerable<int>? indexes)
    {
        var observer = observerGetter.Invoke(obj);
        if (indexes is not null)
        {
            foreach (var (i, @new) in indexes.Zip(newItems)) observer.Replace(i, @new);
        }
        else
        {
            foreach (var (old, @new) in oldItems.Zip(newItems))
            {
                var i = observer.Find(old);
                if (i >= 0) observer.Replace(i, @new);
            }
        }
    }

    public void ListItemMoved(TList obj, IEnumerable<TItem> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes)
    {
        var observer = observerGetter.Invoke(obj);
        foreach (var (from, to) in oldIndexes.Zip(newIndexes)) observer.Move(from, to);
    }

    public void ListReset(TList obj, IReadOnlyList<TItem> listView)
    {
        var observer = observerGetter.Invoke(obj);
        observer.Clear();
        foreach (var item in listView) observer.Add(item);
    }
}