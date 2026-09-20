using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

internal class ObservableDictionaryWrapper<TKey, TValue>(IReadOnlyNotifiableDictionary<TKey, TValue> dictionary, int notifyThreshold) : IReadOnlyObservableDictionary<TKey, TValue>
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => dictionary.Count;

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => dictionary.TryGetValue(key, out value);

    public TValue this[TKey key] => dictionary[key];

    public IReadOnlyObservableCollection<TKey> Keys => field ??= dictionary.Keys.AsReadOnlyObservableCollection(notifyThreshold);
    public IReadOnlyObservableCollection<TValue> Values => field ??= dictionary.Values.AsReadOnlyObservableCollection(notifyThreshold);


    // Events

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => dictionary.DictionaryUpdated += value is null ? null : CollectionChangedHandlerCache.WrapNew(value);
        remove => dictionary.DictionaryUpdated -= value is null ? null : CollectionChangedHandlerCache.RemoveWrapped(value);
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => dictionary.DictionaryUpdated += value is null ? null : PropertyChangedHandlerCache.WrapNew(value);
        remove => dictionary.DictionaryUpdated -= value is null ? null : PropertyChangedHandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        NotifyCollectionChangedEventHandler,
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>
    > CollectionChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        IEnumerable<NotifyCollectionChangedEventArgs> newArgs = args switch
        {
            IDictionaryItemAddedEventArgs<TKey, TValue> added => (notifyThreshold, added.AddedItems) switch
            {
                var (t, items) when t < 0 || items.Count <= t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            IDictionaryItemRemovedEventArgs<TKey, TValue> removed => (notifyThreshold, removed.RemovedItems) switch
            {
                var (t, items) when t < 0 || items.Count <= t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            IDictionaryItemReplacedEventArgs<TKey, TValue> replaced => (notifyThreshold, replaced.ItemChanges) switch
            {
                var (t, changes) when t < 0 || changes.Count <= t =>
                    changes.Select(change => new NotifyCollectionChangedEventArgs(
                                       NotifyCollectionChangedAction.Replace, change.New, change.Old)
                    ),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },

            _ => []
        };

        newArgs.ForEach(a => handler.Invoke(this, a));
    });

    private CountedWeakRefWrapper<
        PropertyChangedEventHandler,
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>
    > PropertyChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        const string indexerName = "Item[]";

        IEnumerable<PropertyChangedEventArgs> newArgs = args switch
        {
            IDictionaryItemAddedEventArgs<TKey, TValue> or IDictionaryItemRemovedEventArgs<TKey, TValue> =>
            [
                new(nameof(Count)),
                new(indexerName)
            ],
            IDictionaryItemReplacedEventArgs<TKey, TValue> =>
            [
                new(indexerName)
            ],

            _ => []
        };

        newArgs.ForEach(a => handler.Invoke(this, a));
    });
}