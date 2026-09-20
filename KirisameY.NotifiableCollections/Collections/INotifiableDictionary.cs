using System.Diagnostics.CodeAnalysis;

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
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated;

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
    event EventHandler<CollectionUpdateEventArgs<KeyValuePair<TKey, TValue>>>? ICollectionUpdateNotifier<KeyValuePair<TKey, TValue>>.CollectionUpdated
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

/// <summary>
///     可读写、且可以订阅变更通知的字典。
///     <br/>
///     A mutable dictionary whose changes can be subscribed to.
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
public interface INotifiableDictionary<TKey, TValue> : INotifiableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IReadOnlyNotifiableDictionary<TKey, TValue>
{
    /// <summary>
    ///     按 key 读写值。
    ///     <br/>
    ///     Reads or writes the value associated with a key.
    /// </summary>
    /// <remarks>
    ///     set 时按 key 是否已存在区分：已存在触发替换更新，不存在触发添加更新。
    ///     <br/>
    ///     On set, the behaviour depends on whether the key already exists: an existing key raises a
    ///     "replace" update, a missing one raises an "add" update.
    /// </remarks>
    public new TValue this[TKey key] { get; set; }

    /// <inheritdoc cref="INotifiableCollection{T}.Count"/>
    public new int Count { get; }

    /// <inheritdoc cref="IReadOnlyNotifiableDictionary{TKey, TValue}.Keys"/>
    public new IReadOnlyNotifiableCollection<TKey> Keys { get; }

    /// <inheritdoc cref="IReadOnlyNotifiableDictionary{TKey, TValue}.Values"/>
    public new IReadOnlyNotifiableCollection<TValue> Values { get; }

    /// <summary>
    ///     判断 key 是否存在。
    ///     <br/>
    ///     Determines whether the dictionary contains the specified key.
    /// </summary>
    public new bool ContainsKey(TKey key);

    /// <summary>
    ///     尝试取出 key 对应的值。
    ///     <br/>
    ///     Tries to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">
    ///     要查找的键。
    ///     <br/>
    ///     The key to look up.
    /// </param>
    /// <param name="value">
    ///     找到时为该键对应的值，否则为 <see langword="default"/>。
    ///     <br/>
    ///     The associated value when found; otherwise <see langword="default"/>.
    /// </param>
    /// <returns>
    ///     找到返回 <see langword="true"/>。
    ///     <br/>
    ///     <see langword="true"/> when the key was found.
    /// </returns>
    public new bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);


    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];
    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => Count;
    int ICollection<KeyValuePair<TKey, TValue>>.Count => Count;

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);
    bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
}