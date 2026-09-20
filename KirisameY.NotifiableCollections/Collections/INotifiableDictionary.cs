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
    /// <remarks>
    ///     该字典在集合视角下是 <c>KeyValuePair&lt;TKey, TValue&gt;</c> 的集合，
    ///     所以本接口继承自 <see cref="ICollectionUpdateNotifier{T}"/>；
    ///     这里的事件参数额外携带字典语义的视图与变更项。
    ///     <br/>
    ///     Seen as a collection, the dictionary is a collection of
    ///     <c>KeyValuePair&lt;TKey, TValue&gt;</c>, which is why this interface derives from
    ///     <see cref="ICollectionUpdateNotifier{T}"/>; the event args here additionally carry a
    ///     dictionary-shaped view and change items.
    /// </remarks>
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated;

    /// <summary>
    ///     与 <see cref="DictionaryUpdated"/> 是<b>同一个</b>事件。
    ///     <br/>
    ///     This is the <b>same</b> event as <see cref="DictionaryUpdated"/>.
    /// </summary>
    /// <remarks>
    ///     显式实现而不是另开一个事件，是为了让字典被当作
    ///     <see cref="ICollectionUpdateNotifier{T}"/> 使用时也能收到通知；
    ///     订阅它等价于订阅 <see cref="DictionaryUpdated"/>。
    ///     <br/>
    ///     It is implemented explicitly rather than declared as a separate event so that the dictionary
    ///     still delivers notifications when it is used as an <see cref="ICollectionUpdateNotifier{T}"/>;
    ///     subscribing to it is equivalent to subscribing to <see cref="DictionaryUpdated"/>.
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
    ///     <c>new</c> 是为了把返回类型从 <see cref="IReadOnlyDictionary{TKey, TValue}"/> 的
    ///     <c>IEnumerable&lt;TKey&gt;</c> 收窄成可通知的集合，下面用显式实现桥接回原签名。
    ///     <br/>
    ///     <c>new</c> narrows the return type from <see cref="IReadOnlyDictionary{TKey, TValue}"/>'s
    ///     <c>IEnumerable&lt;TKey&gt;</c> down to a notifiable collection; the explicit implementation
    ///     below bridges the original signature back.
    /// </remarks>
    public new IReadOnlyNotifiableCollection<TKey> Keys { get; }

    /// <summary>
    ///     值的集合，是一个随字典变更一同发通知的只读视图。
    ///     <br/>
    ///     The set of values, as a read-only view that raises notifications along with the dictionary.
    /// </summary>
    /// <remarks>
    ///     与 <see cref="Keys"/> 同理，用 <c>new</c> 收窄返回类型。
    ///     <br/>
    ///     As with <see cref="Keys"/>, <c>new</c> narrows the return type.
    /// </remarks>
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
    ///     set 时按 key 是否已存在区分：已存在触发「替换」，不存在触发「添加」。
    ///     用 <c>new</c> 把 <see cref="IDictionary{TKey, TValue}"/> 与
    ///     <see cref="IReadOnlyDictionary{TKey, TValue}"/> 两份同名索引器收敛成一个。
    ///     <br/>
    ///     On set, the behaviour depends on whether the key already exists: an existing key raises a
    ///     "replace" change, a missing one raises an "add" change. <c>new</c> is used to collapse the two
    ///     identically named indexers inherited from <see cref="IDictionary{TKey, TValue}"/> and
    ///     <see cref="IReadOnlyDictionary{TKey, TValue}"/> into a single one.
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
    /// <remarks>
    ///     两个基接口各有一份同名方法，用 <c>new</c> 收敛成一个，避免调用点产生歧义。
    ///     <br/>
    ///     Both base interfaces declare a method with this name; <c>new</c> collapses them into one so
    ///     that call sites stay unambiguous.
    /// </remarks>
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
    /// <remarks>
    ///     与 <see cref="ContainsKey"/> 同理，用 <c>new</c> 收敛两个基接口的同名方法。
    ///     <br/>
    ///     As with <see cref="ContainsKey"/>, <c>new</c> collapses the identically named methods of the
    ///     two base interfaces.
    /// </remarks>
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