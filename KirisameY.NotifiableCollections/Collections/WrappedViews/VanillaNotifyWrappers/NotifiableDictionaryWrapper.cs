using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

internal class NotifiableDictionaryWrapper<TKey, TValue, TDict>(TDict dict) : IReadOnlyNotifiableDictionary<TKey, TValue>
    where TKey : notnull
    where TDict : IReadOnlyDictionary<TKey, TValue>, INotifyCollectionChanged
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => dict.Count;
    public bool ContainsKey(TKey key) => dict.ContainsKey(key);
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => dict.TryGetValue(key, out value);
    public TValue this[TKey key] => dict[key];

    public IReadOnlyNotifiableCollection<TKey> Keys => field ??= new NotifiableDictionaryKeySet<TKey, TValue>(this);
    public IReadOnlyNotifiableCollection<TValue> Values => field ??= this.AsReadOnlyNotifiableCollection(p => p.Value);


    //event
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated
    {
        add => dict.CollectionChanged += value is null ? null : PropertyChangedHandlerCache.WrapNew(value);
        remove => dict.CollectionChanged -= value is null ? null : PropertyChangedHandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>,
        NotifyCollectionChangedEventHandler
    > PropertyChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        DictionaryUpdateEventArgs<TKey, TValue>? newArgs = args switch
        {
            {
                Action: NotifyCollectionChangedAction.Add,
                NewItems: { } newItems
            } => new DictionaryItemAddedEventArgs<TKey, TValue>(
                this, newItems.Cast<KeyValuePair<TKey, TValue>>().ToDictionary()
            ),
            {
                Action: NotifyCollectionChangedAction.Remove,
                OldItems: { } oldItems
            } => new DictionaryItemRemovedEventArgs<TKey, TValue>(
                this, oldItems.Cast<KeyValuePair<TKey, TValue>>().ToDictionary()
            ),
            {
                Action: NotifyCollectionChangedAction.Replace,
                OldItems: { } oldItems,
                NewItems: { } newItems
            } => new DictionaryItemReplacedEventArgs<TKey, TValue>(
                this,
                oldItems.Cast<KeyValuePair<TKey, TValue>>().ToDictionary(),
                newItems.Cast<KeyValuePair<TKey, TValue>>().ToDictionary()
            ),
            {
                Action: NotifyCollectionChangedAction.Reset
            } => new DictionaryResetEventArgs<TKey, TValue>(this),
            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}