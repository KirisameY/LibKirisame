namespace KirisameY.NotifiableCollections.Collections;

public interface IListUpdateNotifier<T> : ICollectionUpdateNotifier<T>
{
    public new event EventHandler<ListItemAddedEventArgs<T>>? ItemAdded;
    public new event EventHandler<ListItemRemovedEventArgs<T>>? ItemRemoved;
    public new event EventHandler<ListItemReplacedEventArgs<T>>? ItemReplaced;
    public event EventHandler<ListSortedEventArgs<T>>? Sorted;

    event EventHandler<CollectionItemRemovedEventArgs<T>>? ICollectionUpdateNotifier<T>.ItemRemoved
    {
        add => ItemRemoved += value;
        remove => ItemRemoved -= value;
    }
    event EventHandler<CollectionItemReplacedEventArgs<T>>? ICollectionUpdateNotifier<T>.ItemReplaced
    {
        add => ItemReplaced += value;
        remove => ItemReplaced -= value;
    }
    event EventHandler<CollectionItemAddedEventArgs<T>>? ICollectionUpdateNotifier<T>.ItemAdded
    {
        add => ItemAdded += value;
        remove => ItemAdded -= value;
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