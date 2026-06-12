using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

public interface IDictionaryUpdateNotifier<TKey, TValue> : ICollectionUpdateNotifier<KeyValuePair<TKey, TValue>>
{
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated;

    event EventHandler<CollectionUpdateEventArgs<KeyValuePair<TKey, TValue>>>? ICollectionUpdateNotifier<KeyValuePair<TKey, TValue>>.CollectionUpdated
    {
        add => DictionaryUpdated += value;
        remove => DictionaryUpdated -= value;
    }
}

public interface IReadOnlyNotifiableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionaryUpdateNotifier<TKey, TValue>;

public interface INotifiableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyNotifiableDictionary<TKey, TValue>
{
    public new TValue this[TKey key] { get; set; }
    public new int Count { get; }
    public new ICollection<TKey> Keys { get; }
    public new ICollection<TValue> Values { get; }

    public new bool ContainsKey(TKey key);
    public new bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);


    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];
    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => Count;
    int ICollection<KeyValuePair<TKey, TValue>>.Count => Count;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);
    bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
}