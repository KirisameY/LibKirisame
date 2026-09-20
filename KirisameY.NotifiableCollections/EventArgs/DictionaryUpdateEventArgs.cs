using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

#region Interfaces

/// <summary>
///     字典变更事件参数的基接口，只保证能拿到字典视图。
///     <br/>
///     Base interface for dictionary change event args; it only guarantees access to the dictionary view.
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
/// <remarks>
///     字典在集合视角下是 <c>KeyValuePair&lt;TKey, TValue&gt;</c> 的集合，
///     因此继承自 <see cref="ICollectionUpdateEventArgs{T}"/>；下面各接口用 <c>new</c>
///     把变更项从「键值对集合」收窄成「字典」，以便按键取值。
///     <br/>
///     Seen as a collection, a dictionary is a collection of <c>KeyValuePair&lt;TKey, TValue&gt;</c>, hence the
///     inheritance from <see cref="ICollectionUpdateEventArgs{T}"/>. The interfaces below use <c>new</c> to
///     narrow change items from a "collection of key-value pairs" down to a "dictionary" so that values can be
///     looked up by key.
/// </remarks>
[PublicAPI]
public interface IDictionaryUpdateEventArgs<TKey, TValue> : ICollectionUpdateEventArgs<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    ///     触发本次变更的字典视图，反映变更<b>之后</b>的状态。
    ///     <br/>
    ///     The dictionary view that raised this change, reflecting the state <b>after</b> the change.
    /// </summary>
    /// <remarks>
    ///     与基接口的视图同理，是<b>活视图</b>而非快照。
    ///     <br/>
    ///     As with the base interface's view, this is a <b>live view</b>, not a snapshot.
    /// </remarks>
    [PublicAPI] public IReadOnlyDictionary<TKey, TValue> DictionaryView { get; }
}

/// <summary>
///     一个或多个元素被添加至字典。
///     <br/>
///     One or more elements has added to the dictionary.
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
[PublicAPI]
public interface IDictionaryItemAddedEventArgs<TKey, TValue> : IDictionaryUpdateEventArgs<TKey, TValue>, ICollectionItemAddedEventArgs<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    ///     本次被添加的键值对，按键组织以便直接查找。
    ///     <br/>
    ///     The key-value pairs added by this change, keyed so that they can be looked up directly.
    /// </summary>
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> AddedItems { get; }

    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemAddedEventArgs<KeyValuePair<TKey, TValue>>.AddedItems => AddedItems;
}

/// <summary>
///     一个或多个元素被从字典中移除。
///     <br/>
///     One or more elements has removed from the dictionary.
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
[PublicAPI]
public interface IDictionaryItemRemovedEventArgs<TKey, TValue> : IDictionaryUpdateEventArgs<TKey, TValue>, ICollectionItemRemovedEventArgs<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    ///     本次被移除的键值对（含被移除时的值）.
    ///     <br/>
    ///     The key-value pairs removed by this change (including the values they held when removed).
    /// </summary>
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> RemovedItems { get; }

    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemRemovedEventArgs<KeyValuePair<TKey, TValue>>.RemovedItems => RemovedItems;
}

/// <summary>
///     字典被清空。
///     <br/>
///     The dictionary was cleared.
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
/// <remarks>
///     是「移除」的特例：<see cref="IDictionaryItemRemovedEventArgs{TKey, TValue}.RemovedItems"/> 即清空前的全部键值对。
///     <br/>
///     A special case of "remove": <see cref="IDictionaryItemRemovedEventArgs{TKey, TValue}.RemovedItems"/> holds
///     every key-value pair the dictionary contained before being cleared.
/// </remarks>
[PublicAPI]
public interface IDictionaryItemClearedEventArgs<TKey, TValue> : IDictionaryItemRemovedEventArgs<TKey, TValue>, ICollectionItemClearedEventArgs<KeyValuePair<TKey, TValue>>;

/// <summary>
///     字典中一个或多个键对应的值被替换。
///     <br/>
///     The values of one or more keys in a dictionary has been replaced.
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
[PublicAPI]
public interface IDictionaryItemReplacedEventArgs<TKey, TValue> : IDictionaryUpdateEventArgs<TKey, TValue>, ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    ///     被替换掉的旧键值对，按键组织。
    ///     <br/>
    ///     The old key-value pairs that were replaced, keyed.
    /// </summary>
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> OldItems { get; }

    /// <summary>
    ///     替换后的新键值对，按键组织。
    ///     <br/>
    ///     The new key-value pairs, keyed.
    /// </summary>
    /// <remarks>
    ///     字典是按 key 配对的，不像集合版那样依赖「逐位对应」。
    ///     <br/>
    ///     Dictionary entries are paired by key rather than by position, unlike the collection version.
    /// </remarks>
    [PublicAPI] public new IReadOnlyDictionary<TKey, TValue> NewItems { get; }

    /// <summary>
    ///     每个被替换的键的新旧值对照。
    ///     <br/>
    ///     The old/new value pairs for each replaced key.
    /// </summary>
    /// <remarks>
    ///     以 <see cref="OldItems"/> 的键为准，新值取自 <see cref="NewItems"/> 中同一个键。
    ///     <br/>
    ///     The keys of <see cref="OldItems"/> are authoritative, and the new value is taken from the same key
    ///     in <see cref="NewItems"/>.
    /// </remarks>
    [PublicAPI] public new IReadOnlyCollection<IDictionaryItemReplaceInfo<TKey, TValue>> ItemChanges { get; }

    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>.OldItems => OldItems;
    IReadOnlyCollection<KeyValuePair<TKey, TValue>> ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>.NewItems => NewItems;
    IReadOnlyCollection<IItemReplaceInfo<KeyValuePair<TKey, TValue>>> ICollectionItemReplacedEventArgs<KeyValuePair<TKey, TValue>>.ItemChanges => ItemChanges;
}

#endregion

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
