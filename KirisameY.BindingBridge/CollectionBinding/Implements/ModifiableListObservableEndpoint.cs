using KirisameY.Relinq.Extensions;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class ModifiableListObservableEndpoint<TList, TItem>(
    Func<TList, IList<TItem>> listGetter
) : IListObserverEndpoint<TList, TItem>
    where TList : class
{
    public void ListItemAdded(TList obj, IEnumerable<TItem> added, IEnumerable<int>? indexes)
    {
        var list = listGetter.Invoke(obj);
        if (indexes is null) added.ForEach(list.Add);
        else
        {
            foreach (var (i, item) in indexes.Zip(added)) list.Insert(i, item);
        }
    }

    public void ListItemRemoved(TList obj, IEnumerable<TItem> removed, IEnumerable<int>? indexes)
    {
        var list = listGetter.Invoke(obj);
        if (indexes is not null) indexes.Reverse().ForEach(list.RemoveAt);
        else
        {
            foreach (var item in removed) list.Remove(item);
        }
    }

    public void ListItemReplaced(TList obj, IEnumerable<TItem> oldItems, IEnumerable<TItem> newItems, IEnumerable<int>? indexes)
    {
        var list = listGetter.Invoke(obj);
        if (indexes is not null)
        {
            foreach (var (i, item) in indexes.Zip(newItems)) list[i] = item;
        }
        else
        {
            foreach (var (oldItem, newItem) in oldItems.Zip(newItems))
            {
                var index = list.IndexOf(oldItem);
                list.RemoveAt(index);
                list.Insert(index, newItem);
            }
        }
    }

    public void ListItemMoved(TList obj, IEnumerable<TItem> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes)
    {
        var list = listGetter.Invoke(obj);
        foreach (var (oldIndex, newIndex, item) in oldIndexes.Zip(newIndexes, items))
        {
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
    }

    public void ListReset(TList obj, IReadOnlyList<TItem> listView)
    {
        var list = listGetter.Invoke(obj);
        list.Clear();
        listView.ForEach(list.Add);
    }
}