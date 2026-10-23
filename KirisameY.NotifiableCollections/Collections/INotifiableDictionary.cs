using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.Collections;

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


    #region Update

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <inheritdoc cref="Dictionary{TKey,TValue}.TryAdd" />
    [PublicAPI] bool TryAdd(TKey key, TValue value);

    /// <summary>
    ///     追加一批键值对。
    ///     <br/>
    ///     Adds a batch of key-value pairs.
    /// </summary>
    /// <param name="values">
    ///     要添加的键值对。
    ///     <br/>
    ///     The key-value pairs to add.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     入参里有键已经存在于字典中，或者入参内部有重复的键。
    ///     <br/>
    ///     A key in the argument already exists in the dictionary, or the argument contains duplicate keys.
    /// </exception>
    [PublicAPI] void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> values);

    void INotifiableCollection<KeyValuePair<TKey, TValue>>.AddRange(IReadOnlyCollection<KeyValuePair<TKey, TValue>> elements) => AddRange(elements);

    /// <summary>
    ///     尝试追加一批键值对。
    ///     <br/>
    ///     Tries to add a batch of key-value pairs.
    /// </summary>
    /// <param name="values">
    ///     要添加的键值对。
    ///     <br/>
    ///     The key-value pairs to add.
    /// </param>
    /// <returns>
    ///     被成功添加的键值对集合。
    ///     <br/>
    ///     The pairs that were actually added.
    /// </returns>
    [PublicAPI] IReadOnlyDictionary<TKey, TValue> TryAddRange(IEnumerable<KeyValuePair<TKey, TValue>> values);

    /// <summary>
    ///     写入一批键值对：没有的键按添加处理，已有的键按替换处理。
    ///     <br/>
    ///     Writes a batch of key-value pairs: missing keys are added, existing ones are replaced.
    /// </summary>
    /// <param name="values">
    ///     要写入的键值对。
    ///     <br/>
    ///     The key-value pairs to write.
    /// </param>
    /// <returns>
    ///     被替换掉的键值对集合。
    ///     <br/>
    ///     The pairs that were replaced.
    /// </returns>
    /// <remarks>
    ///     可能发出<b>最多两次</b>通知——替换和通知，替换通知总是会早于添加通知被发出。
    ///     <br/>
    ///     <b>Up to two</b> notifications may be raised — one for the replacements, then one for the additions,
    ///     always in that order.
    /// </remarks>
    [PublicAPI] IReadOnlyDictionary<TKey, TValue> PutRange(IEnumerable<KeyValuePair<TKey, TValue>> values);

    /// <inheritdoc cref="Dictionary{TKey,TValue}.Remove(TKey, out TValue)" />
    [PublicAPI] bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value);

    bool IDictionary<TKey, TValue>.Remove(TKey key) => Remove(key, out _);

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!Contains(item)) return false;
        Remove(item.Key);
        return true;
    }

    /// <summary>
    ///     按 key 移除一批键值对。
    ///     <br/>
    ///     Removes a batch of keys.
    /// </summary>
    /// <param name="keys">
    ///     要移除的键。
    ///     <br/>
    ///     The keys to remove.
    /// </param>
    /// <returns>
    ///     被成功移除的键值对集合。
    ///     <br/>
    ///     The pairs that were actually removed.
    /// </returns>
    /// <remarks>
    ///     不存在的键会被静默跳过，重复的键只有第一次能删到。一个都没删掉时不发通知。
    ///     <br/>
    ///     Keys that are not present are skipped silently, and a repeated key is only removed once. When nothing
    ///     was removed, no notification is raised.
    /// </remarks>
    [PublicAPI] IReadOnlyDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys);

    IReadOnlyCollection<KeyValuePair<TKey, TValue>> INotifiableCollection<KeyValuePair<TKey, TValue>>.RemoveRange(
        IReadOnlyCollection<KeyValuePair<TKey, TValue>> elements
    ) => RemoveRange(elements.Where(Contains).Select(p => p.Key));

    /// <summary>
    ///     移除字典中的所有键值对。
    ///     <br/>
    ///     Remove all KVPs from the dictionary.
    /// </summary>
    /// <returns>
    ///     被移除的键值对集合。
    ///     <br/>
    ///     Collection of removed pairs.
    /// </returns>
    [PublicAPI] new IReadOnlyDictionary<TKey, TValue> Clear();

    void ICollection<KeyValuePair<TKey, TValue>>.Clear() => Clear();

    bool INotifiableCollection<KeyValuePair<TKey, TValue>>.Replace(KeyValuePair<TKey, TValue> oldElement, KeyValuePair<TKey, TValue> newElement)
    {
        if (
            !Equals(oldElement.Key, newElement.Key) ||
            !Contains(oldElement)
        ) return false;

        this[oldElement.Key] = newElement.Value;
        return true;
    }

    IReadOnlyCollection<(bool Replaced, KeyValuePair<TKey, TValue> Left)> INotifiableCollection<KeyValuePair<TKey, TValue>>.ReplaceRange(
        IReadOnlyCollection<(KeyValuePair<TKey, TValue> Old, KeyValuePair<TKey, TValue> New)> replaces)
    {
        List<(bool Replaced, KeyValuePair<TKey, TValue> Left)> result = new(capacity: replaces.Count);
        List<KeyValuePair<TKey, TValue>> replace = new(capacity: replaces.Count);
        foreach (var (old, @new) in replaces)
        {
            if (
                !EqualityComparer<TKey>.Default.Equals(old.Key, @new.Key) ||
                !Contains(old)
            ) result.Add((false, @new));
            else replace.Add(@new);
        }
        var oldItems = PutRange(replace);
        result.AddRange(oldItems.Select(item => (true, item)));
        return [..result];
    }

    #endregion


    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];
    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => Count;
    int ICollection<KeyValuePair<TKey, TValue>>.Count => Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) =>
        TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);
    bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
}