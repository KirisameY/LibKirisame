using System.Collections.Specialized;
using System.ComponentModel;

namespace KirisameY.NotifiableCollections.Collections;

public interface IReadOnlyObservableCollection<out T> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged;

public interface IReadOnlyObservableList<out T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>;

public interface IReadOnlyObservableDictionary<TKey, TValue> : IReadOnlyObservableCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
{
    new IReadOnlyObservableCollection<TKey> Keys { get; }
    new IReadOnlyObservableCollection<TValue> Values { get; }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
}