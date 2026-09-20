using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     字典变更事件参数的公共基类。
///     <br/>
///     Common base class for dictionary change event args.
/// </summary>
/// <typeparam name="TKey">
///     键类型。
///     <br/>
///     The type of the keys.
/// </typeparam>
/// <typeparam name="TValue">
///     值类型。
///     <br/>
///     The type of the values.
/// </typeparam>
/// <param name="dictionaryView">
///     触发本次变更的字典视图。
///     <br/>
///     The dictionary view that raised this change.
/// </param>
public abstract class DictionaryUpdateEventArgs<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionaryView)
    : CollectionUpdateEventArgs<KeyValuePair<TKey, TValue>>(dictionaryView), IDictionaryUpdateEventArgs<TKey, TValue>
{
    /// <inheritdoc cref="IDictionaryUpdateEventArgs{TKey, TValue}.DictionaryView"/>
    public IReadOnlyDictionary<TKey, TValue> DictionaryView => dictionaryView;
}

internal class DictionaryItemAddedEventArgs<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionaryView, IReadOnlyDictionary<TKey, TValue> addedItems)
    : DictionaryUpdateEventArgs<TKey, TValue>(dictionaryView), IDictionaryItemAddedEventArgs<TKey, TValue>
{
    public IReadOnlyDictionary<TKey, TValue> AddedItems => addedItems;
}

internal class DictionaryItemRemovedEventArgs<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionaryView, IReadOnlyDictionary<TKey, TValue> removedItems)
    : DictionaryUpdateEventArgs<TKey, TValue>(dictionaryView), IDictionaryItemRemovedEventArgs<TKey, TValue>
{
    public IReadOnlyDictionary<TKey, TValue> RemovedItems => removedItems;
}

internal class DictionaryItemClearedEventArgs<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionaryView, IReadOnlyDictionary<TKey, TValue> removedItems)
    : DictionaryItemRemovedEventArgs<TKey, TValue>(dictionaryView, removedItems), IDictionaryItemClearedEventArgs<TKey, TValue>;

internal class DictionaryItemReplacedEventArgs<TKey, TValue>(
    IReadOnlyDictionary<TKey, TValue> dictionaryView,
    IReadOnlyDictionary<TKey, TValue> oldItems,
    IReadOnlyDictionary<TKey, TValue> newItems)
    : DictionaryUpdateEventArgs<TKey, TValue>(dictionaryView), IDictionaryItemReplacedEventArgs<TKey, TValue>
{
    public IReadOnlyDictionary<TKey, TValue> OldItems => oldItems;
    public IReadOnlyDictionary<TKey, TValue> NewItems => newItems;

    public IReadOnlyCollection<IDictionaryItemReplaceInfo<TKey, TValue>> ItemChanges => field ??=
    [
        ..OldItems.Select(o => DictionaryItemReplaceInfo.From(o.Key, o.Value, NewItems[o.Key]))
    ];
}
