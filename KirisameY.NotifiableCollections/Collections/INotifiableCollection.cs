namespace KirisameY.NotifiableCollections;

public interface ICollectionUpdateNotifier<T>
{
    public event EventHandler<CollectionItemAddedEventArgs<T>>? ItemAdded;
    public event EventHandler<CollectionItemRemovedEventArgs<T>>? ItemRemoved;
    public event EventHandler<CollectionItemReplacedEventArgs<T>>? ItemReplaced;
}

public interface IReadOnlyNotifiableCollection<T> : IReadOnlyCollection<T>, ICollectionUpdateNotifier<T>;

public interface INotifiableCollection<T> : ICollection<T>, IReadOnlyNotifiableCollection<T>
{
    public new int Count { get; }

    int ICollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;
}