using System.Collections;
using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews;

internal class NotifiableDictionaryReadonlyView<TKey, TValue>(IReadOnlyNotifiableDictionary<TKey, TValue> dictionary)
    : IReadOnlyNotifiableDictionary<TKey, TValue>
    where TKey : notnull
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dictionary).GetEnumerator();

    public bool ContainsKey(TKey key) => ((IReadOnlyDictionary<TKey, TValue>)dictionary).ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value) => ((IReadOnlyDictionary<TKey, TValue>)dictionary).TryGetValue(key, out value!);

    public TValue this[TKey key] => dictionary[key];

    public int Count => dictionary.Count;

    public IReadOnlyNotifiableCollection<TKey> Keys => dictionary.Keys;

    public IReadOnlyNotifiableCollection<TValue> Values => dictionary.Values;


    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated
    {
        add => dictionary.DictionaryUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => dictionary.DictionaryUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>,
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        DictionaryUpdateEventArgs<TKey, TValue>? newArgs = args switch
        {
            IDictionaryItemAddedEventArgs<TKey, TValue> added => new DictionaryItemAddedEventArgs<TKey, TValue>(
                this, added.AddedItems
            ),
            IDictionaryItemClearedEventArgs<TKey, TValue> cleared => new DictionaryItemClearedEventArgs<TKey, TValue>(
                this, cleared.RemovedItems
            ),
            IDictionaryItemRemovedEventArgs<TKey, TValue> removed => new DictionaryItemRemovedEventArgs<TKey, TValue>(
                this, removed.RemovedItems
            ),
            IDictionaryItemReplacedEventArgs<TKey, TValue> replaced => new DictionaryItemReplacedEventArgs<TKey, TValue>(
                this, replaced.OldItems, replaced.NewItems
            ),

            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}

internal class NotifiableDictionaryReadOnlyView<TKey, TSourceValue, TValue>(
    IReadOnlyNotifiableDictionary<TKey, TSourceValue> dictionary, Func<TSourceValue, TValue> valueSelector
) : IReadOnlyNotifiableDictionary<TKey, TValue>
    where TKey : notnull
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.Select(kvp => new KeyValuePair<TKey, TValue>(kvp.Key, valueSelector.Invoke(kvp.Value))).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => dictionary.Count;

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var result = dictionary.TryGetValue(key, out var v);
        value = result ? valueSelector.Invoke(v!) : default;
        return result;
    }

    public TValue this[TKey key] => valueSelector.Invoke(dictionary[key]);
    public IReadOnlyNotifiableCollection<TKey> Keys => dictionary.Keys;
    public IReadOnlyNotifiableCollection<TValue> Values => field ??= dictionary.Values.AsReadOnlyNotifiableCollection(valueSelector);

    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated
    {
        add => dictionary.DictionaryUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => dictionary.DictionaryUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }


    private CountedWeakRefWrapper<
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>,
        EventHandler<DictionaryUpdateEventArgs<TKey, TSourceValue>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        DictionaryUpdateEventArgs<TKey, TValue>? newArgs = args switch
        {
            IDictionaryItemAddedEventArgs<TKey, TSourceValue> added => new DictionaryItemAddedEventArgs<TKey, TValue>(
                this,
                added.AddedItems.ToDictionary(p => p.Key, p => valueSelector.Invoke(p.Value))
            ),
            IDictionaryItemClearedEventArgs<TKey, TSourceValue> cleared => new DictionaryItemClearedEventArgs<TKey, TValue>(
                this,
                cleared.RemovedItems.ToDictionary(p => p.Key, p => valueSelector.Invoke(p.Value))
            ),
            IDictionaryItemRemovedEventArgs<TKey, TSourceValue> removed => new DictionaryItemRemovedEventArgs<TKey, TValue>(
                this,
                removed.RemovedItems.ToDictionary(p => p.Key, p => valueSelector.Invoke(p.Value))
            ),
            IDictionaryItemReplacedEventArgs<TKey, TSourceValue> replaced => new DictionaryItemReplacedEventArgs<TKey, TValue>(
                this,
                replaced.OldItems.ToDictionary(p => p.Key, p => valueSelector.Invoke(p.Value)),
                replaced.NewItems.ToDictionary(p => p.Key, p => valueSelector.Invoke(p.Value))
            ),

            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}