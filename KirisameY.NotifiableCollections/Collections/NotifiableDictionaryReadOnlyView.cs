using System.Collections;
using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

internal class NotifiableDictionaryReadOnlyView<TKey, TValue>(INotifiableDictionary<TKey, TValue> dictionary)
    : IReadOnlyNotifiableDictionary<TKey, TValue>
    where TKey : notnull
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

internal class NotifiableDictionaryReadOnlyView<TKey, TSourceValue, TValue>(IReadOnlyNotifiableDictionary<TKey, TSourceValue> dictionary)
    : IReadOnlyNotifiableDictionary<TKey, TValue>
    where TSourceValue : TValue where TKey : notnull
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.Select(kvp => new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => dictionary.Count;

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var result = dictionary.TryGetValue(key, out var v);
        value = v;
        return result;
    }

    public TValue this[TKey key] => dictionary[key];
    public IEnumerable<TKey> Keys => dictionary.Keys;
    public IEnumerable<TValue> Values => dictionary.Values.Select(v => (TValue)v);

    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated
    {
        add => dictionary.DictionaryUpdated += Wrap(value);
        remove => dictionary.DictionaryUpdated -= RemoveWrap(value);
    }

    private readonly Dictionary<
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>,
        EventHandler<DictionaryUpdateEventArgs<TKey, TSourceValue>>
    > _cachedWraps = [];

    private EventHandler<DictionaryUpdateEventArgs<TKey, TSourceValue>>? Wrap(EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? handler)
    {
        if (handler is null) return null;
        _cachedWraps.Add(handler, (sender, args) =>
        {
            DictionaryUpdateEventArgs<TKey, TValue> newArgs = args switch
            {
                DictionaryItemAddedEventArgs<TKey, TSourceValue> added => new DictionaryItemAddedEventArgs<TKey, TValue>(
                    this,
                    added.AddedItems.ToDictionary(p => p.Key, p => (TValue)p.Value)
                ),
                DictionaryItemClearedEventArgs<TKey, TSourceValue> cleared => new DictionaryItemClearedEventArgs<TKey, TValue>(
                    this,
                    cleared.RemovedItems.ToDictionary(p => p.Key, p => (TValue)p.Value)
                ),
                DictionaryItemRemovedEventArgs<TKey, TSourceValue> removed => new DictionaryItemRemovedEventArgs<TKey, TValue>(
                    this,
                    removed.RemovedItems.ToDictionary(p => p.Key, p => (TValue)p.Value)
                ),
                DictionaryItemReplacedEventArgs<TKey, TSourceValue> replaced => new DictionaryItemReplacedEventArgs<TKey, TValue>(
                    this,
                    replaced.OldItems.ToDictionary(p => p.Key, p => (TValue)p.Value),
                    replaced.NewItems.ToDictionary(p => p.Key, p => (TValue)p.Value)
                ),
                _ => throw new InvalidDataException($"Unexpected type {args.GetType()} of args")
            };
            handler.Invoke(sender, newArgs);
        });
        return _cachedWraps[handler];
    }

    private EventHandler<DictionaryUpdateEventArgs<TKey, TSourceValue>>? RemoveWrap(EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? handler)
    {
        if (handler is null) return null;
        _cachedWraps.Remove(handler, out var result);
        return result;
    }
}