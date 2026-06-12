using System.Collections;

using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

internal class NotifiableListReadOnlyView<T>(INotifiableList<T> list) : IReadOnlyNotifiableList<T>
{
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();
    public int Count => list.Count;
    public T this[int index] => list[index];


    public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated
    {
        add => list.ListUpdated += value;
        remove => list.ListUpdated -= value;
    }
}