using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

public interface ICollectionUpdateNotifier<T>
{
    public event EventHandler<CollectionUpdateEventArgs<T>>? CollectionUpdated;
}

public interface IReadOnlyNotifiableCollection<T> : IReadOnlyCollection<T>, ICollectionUpdateNotifier<T>;

public interface INotifiableCollection<T> : ICollection<T>, IReadOnlyNotifiableCollection<T>
{
    public new int Count { get; }

    int ICollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;
}