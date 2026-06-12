using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

#region Interfaces

[PublicAPI]
public interface IDictionaryUpdateEventArgs<TKey, TValue> : ICollectionUpdateEventArgs<KeyValuePair<TKey, TValue>>
{
    [PublicAPI] public IReadOnlyDictionary<TKey, TValue> DictionaryView { get; }
}

[PublicAPI]
public interface IDictionaryItemAddedEventArgs<TKey, TValue> : IDictionaryUpdateEventArgs<TKey, TValue>, ICollectionItemAddedEventArgs<KeyValuePair<TKey, TValue>>
{
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> AddedItems { get; }

    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemAddedEventArgs<KeyValuePair<TKey, TValue>>.AddedItems => AddedItems;
}

[PublicAPI]
public interface IDictionaryItemRemovedEventArgs<TKey, TValue> : IDictionaryUpdateEventArgs<TKey, TValue>, ICollectionItemRemovedEventArgs<KeyValuePair<TKey, TValue>>
{
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> RemovedItems { get; }

    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemRemovedEventArgs<KeyValuePair<TKey, TValue>>.RemovedItems => RemovedItems;
}

[PublicAPI]
public interface IDictionaryItemClearedEventArgs<TKey, TValue> : IDictionaryItemRemovedEventArgs<TKey, TValue>;

[PublicAPI]
public interface IDictionaryItemReplacedEventArgs<TKey, TValue> : IDictionaryUpdateEventArgs<TKey, TValue>, ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>
{
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> OldItems { get; }
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> NewItems { get; }
    [PublicAPI] public new IReadOnlyCollection<IDictionaryItemReplaceInfo<TKey, TValue>> ItemChanges { get; }

    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>.OldItems => OldItems;
    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>.NewItems => NewItems;
    IReadOnlyCollection<IItemReplaceInfo<KeyValuePair<TKey, TValue>>> ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>.ItemChanges => ItemChanges;
}

#endregion

public abstract class DictionaryUpdateEventArgs<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionaryView)
    : CollectionUpdateEventArgs<KeyValuePair<TKey, TValue>>(dictionaryView), IDictionaryUpdateEventArgs<TKey, TValue>
{
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