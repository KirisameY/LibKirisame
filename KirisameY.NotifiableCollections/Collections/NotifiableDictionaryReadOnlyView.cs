using System.Collections;

using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

internal class NotifiableDictionaryReadOnlyView<TKey, TValue>(INotifiableDictionary<TKey, TValue> dictionary) : IReadOnlyNotifiableDictionary<TKey, TValue>
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dictionary).GetEnumerator();

    public bool ContainsKey(TKey key) => ((IReadOnlyDictionary<TKey, TValue>)dictionary).ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value) => ((IReadOnlyDictionary<TKey, TValue>)dictionary).TryGetValue(key, out value!);

    public TValue this[TKey key] => dictionary[key];

    public int Count => dictionary.Count;

    public IEnumerable<TKey> Keys => dictionary.Keys;

    public IEnumerable<TValue> Values => dictionary.Values;


    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated
    {
        add => dictionary.DictionaryUpdated += value;
        remove => dictionary.DictionaryUpdated -= value;
    }
}
