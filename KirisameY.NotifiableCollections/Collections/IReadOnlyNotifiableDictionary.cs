using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可订阅字典变更通知的字典。
///     <br/>
///     A dictionary whose changes can be subscribed to.
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
public interface IDictionaryUpdateNotifier<TKey, TValue> : ICollectionUpdateNotifier<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    ///     字典内容发生变化时触发。
    ///     <br/>
    ///     Raised when the contents of the dictionary change.
    /// </summary>
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>> DictionaryUpdated;

    /// <summary>
    ///     与 <see cref="DictionaryUpdated"/> 是<b>同一个</b>事件。
    ///     <br/>
    ///     This is the <b>same</b> event as <see cref="DictionaryUpdated"/>.
    /// </summary>
    /// <remarks>
    ///     订阅它等价于订阅 <see cref="DictionaryUpdated"/>。
    ///     <br/>
    ///     Subscribing to it is equivalent to subscribing to <see cref="DictionaryUpdated"/>.
    /// </remarks>
    event EventHandler<CollectionUpdateEventArgs<KeyValuePair<TKey, TValue>>> ICollectionUpdateNotifier<KeyValuePair<TKey, TValue>>.CollectionUpdated
    {
        add => DictionaryUpdated += value;
        remove => DictionaryUpdated -= value;
    }
}

/// <summary>
///     只读、但可以订阅变更通知的字典。
///     <br/>
///     A read-only dictionary whose changes can be subscribed to.
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
public interface IReadOnlyNotifiableDictionary<TKey, TValue> : IReadOnlyNotifiableCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IDictionaryUpdateNotifier<TKey, TValue>
{
    /// <summary>
    ///     键的集合，是一个随字典变更一同发通知的只读视图。
    ///     <br/>
    ///     The set of keys, as a read-only view that raises notifications along with the dictionary.
    /// </summary>
    /// <remarks>
    ///     只有键被增删时才会发送通知，仅替换某个键的值不会触发任何通知。
    ///     <br/>
    ///     It only reacts to keys being added or removed, not to a value being
    ///     replaced for an existing key.
    /// </remarks>
    public new IReadOnlyNotifiableCollection<TKey> Keys { get; }

    /// <summary>
    ///     值的集合，是一个随字典变更一同发通知的只读视图。
    ///     <br/>
    ///     The set of values, as a read-only view that raises notifications along with the dictionary.
    /// </summary>
    public new IReadOnlyNotifiableCollection<TValue> Values { get; }


    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
}