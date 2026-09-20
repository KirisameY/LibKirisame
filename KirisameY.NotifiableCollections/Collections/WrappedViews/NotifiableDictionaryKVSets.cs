using System.Collections;
using System.Collections.Immutable;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews;

internal class NotifiableDictionaryKeySet<TKey, TValue>(IReadOnlyNotifiableDictionary<TKey, TValue> dictionary) : IReadOnlyNotifiableCollection<TKey>
{
    public IEnumerator<TKey> GetEnumerator() => dictionary.Select(p => p.Key).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => dictionary.Count;

    public event EventHandler<CollectionUpdateEventArgs<TKey>>? CollectionUpdated
    {
        add => dictionary.DictionaryUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => dictionary.DictionaryUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<CollectionUpdateEventArgs<TKey>>,
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        CollectionUpdateEventArgs<TKey>? newArgs = args switch
        {
            IDictionaryItemAddedEventArgs<TKey, TValue> added => new CollectionItemAddedEventArgs<TKey>(
                this,
                [..added.AddedItems.Select(p => p.Key)]
            ),
            IDictionaryItemClearedEventArgs<TKey, TValue> cleared => new CollectionItemClearedEventArgs<TKey>(
                this,
                [..cleared.RemovedItems.Select(p => p.Key)]
            ),
            IDictionaryItemRemovedEventArgs<TKey, TValue> removed => new CollectionItemRemovedEventArgs<TKey>(
                this,
                [..removed.RemovedItems.Select(p => p.Key)]
            ),
            _ => null, // 字典替换不影响 Key
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}