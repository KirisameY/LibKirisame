using System.Collections;

namespace KirisameY.NotifiableCollections.Collections;

internal class NotifiableListReadOnlyView<T>(INotifiableList<T> list) : IReadOnlyNotifiableList<T>
{
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();
    public int Count => list.Count;
    public T this[int index] => list[index];


    public event EventHandler<ListItemAddedEventArgs<T>>? ItemAdded
    {
        add => list.ItemAdded += value;
        remove => list.ItemAdded -= value;
    }
    public event EventHandler<ListSortedEventArgs<T>>? Sorted
    {
        add => list.Sorted += value;
        remove => list.Sorted -= value;
    }
    public event EventHandler<ListItemRemovedEventArgs<T>>? ItemRemoved
    {
        add => list.ItemRemoved += value;
        remove => list.ItemRemoved -= value;
    }
    public event EventHandler<ListItemReplacedEventArgs<T>>? ItemReplaced
    {
        add => list.ItemReplaced += value;
        remove => list.ItemReplaced -= value;
    }
}