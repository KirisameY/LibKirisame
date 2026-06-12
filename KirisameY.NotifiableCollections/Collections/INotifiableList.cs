using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

public interface IListUpdateNotifier<T> : ICollectionUpdateNotifier<T>
{
    public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated;

    event EventHandler<CollectionUpdateEventArgs<T>>? ICollectionUpdateNotifier<T>.CollectionUpdated
    {
        add => ListUpdated += value;
        remove => ListUpdated -= value;
    }
}

public interface IReadOnlyNotifiableList<T> : IReadOnlyList<T>, IListUpdateNotifier<T>;

public interface INotifiableList<T> : IList<T>, IReadOnlyNotifiableList<T>
{
    public new int Count { get; }
    public new T this[int index] { get; set; }


    T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }
    T IReadOnlyList<T>.this[int index] => this[index];

    int ICollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;
}