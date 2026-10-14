using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.Collections.WrappedViews;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可通知的字典：在 <see cref="IDictionary{TKey, TValue}"/> 的基础上，
///     把每次改动作为 <see cref="DictionaryUpdated"/> 事件发出去。
///     <br/>
///     A notifiable dictionary: an <see cref="IDictionary{TKey, TValue}"/> that publishes every change through
///     the <see cref="DictionaryUpdated"/> event.
/// </summary>
/// <typeparam name="TKey">
///     键的类型。
///     <br/>
///     The type of the keys.
/// </typeparam>
/// <typeparam name="TValue">
///     值的类型。
///     <br/>
///     The type of the values.
/// </typeparam>
/// <remarks>
///     <para>
///         变更以键值对为单位报告（见 <see cref="IDictionaryUpdateEventArgs{TKey, TValue}"/> 一族）.
///         <br/>
///         <see cref="Keys"/> 与 <see cref="Values"/> 本身就是可通知的视图，可以只订阅其中一边。
///     </para>
///     <para>
///         Changes are reported in terms of key-value pairs (see the <see cref="IDictionaryUpdateEventArgs{TKey, TValue}"/> family).
///         <br/>
///         <see cref="Keys"/> and <see cref="Values"/> are notifying views in their own right, so either side can be independently subscribed to.
///     </para>
/// </remarks>
public class NotifiableDictionary<TKey, TValue> : INotifiableDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _innerDict = [];

    private IReadOnlyDictionary<TKey, TValue> Readonly => field ??= _innerDict.AsReadOnly();


    /// <inheritdoc cref="INotifiableDictionary{TKey,TValue}.this" />
    public TValue this[TKey key]
    {
        get => _innerDict[key];
        set
        {
            if (_innerDict.TryGetValue(key, out var oldValue))
            {
                _innerDict[key] = value;
                RaiseUpdate(new DictionaryItemReplacedEventArgs<TKey, TValue>(
                                Readonly,
                                new Dictionary<TKey, TValue> { { key, oldValue } }.AsReadOnly(),
                                new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                            ));
            }
            else
            {
                _innerDict[key] = value;
                RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                                Readonly,
                                new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                            ));
            }
        }
    }


    #region Reading

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _innerDict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_innerDict).GetEnumerator();

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.ContainsKey" />
    public bool ContainsKey(TKey key) => _innerDict.ContainsKey(key);

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.TryGetValue" />
    public bool TryGetValue(TKey key, out TValue value) => _innerDict.TryGetValue(key, out value!);

    /// <inheritdoc cref="IReadOnlyCollection{T}.Count" />
    public int Count => _innerDict.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    /// <inheritdoc cref="INotifiableDictionary{TKey,TValue}.Keys" />
    public IReadOnlyNotifiableCollection<TKey> Keys => field ??= new NotifiableDictionaryKeySet<TKey, TValue>(this);

    /// <inheritdoc cref="INotifiableDictionary{TKey,TValue}.Values" />
    public IReadOnlyNotifiableCollection<TValue> Values => field ??= this.AsReadOnlyNotifiableCollection(p => p.Value);

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _innerDict.Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => _innerDict.Values;

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item) => _innerDict.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerDict).CopyTo(array, arrayIndex);

    #endregion


    #region Writing

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
        _innerDict.Add(key, value);
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
    }

    /// <summary>
    ///     添加一个键值对，结果与 <see cref="Add(TKey, TValue)"/> 完全相同。
    ///     <br/>
    ///     Adds a key-value pair; identical in effect to <see cref="Add(TKey, TValue)"/>.
    /// </summary>
    /// <param name="item">
    ///     要添加的键值对。
    ///     <br/>
    ///     The key-value pair to add.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     键已存在。
    ///     <br/>
    ///     The key already exists.
    /// </exception>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

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
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        var dict = values.ToImmutableDictionary();
        if (dict.IsEmpty) return;

        var items = dict.ToArray();
        foreach (var (index, pair) in items.Index())
        {
            try
            {
                _innerDict.Add(pair.Key, pair.Value);
            }
            catch (ArgumentException)
            {
                for (int i = 0; i < index; i++) _innerDict.Remove(items[i].Key);
                throw;
            }
        }
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        Readonly, dict
                    ));
    }

    /// <inheritdoc cref="Dictionary{TKey,TValue}.TryAdd" />
    public bool TryAdd(TKey key, TValue value)
    {
        if (!_innerDict.TryAdd(key, value)) return false;
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
        return true;
    }

    /// <summary>
    ///     尝试添加一个键值对，结果与 <see cref="TryAdd(TKey, TValue)"/> 完全相同。
    ///     <br/>
    ///     Tries to add a key-value pair; identical in effect to <see cref="TryAdd(TKey, TValue)"/>.
    /// </summary>
    /// <param name="item">
    ///     要添加的键值对。
    ///     <br/>
    ///     The key-value pair to add.
    /// </param>
    /// <returns>
    ///     添加成功返回 <see langword="true"/>；键已存在则返回 <see langword="false"/>。
    ///     <br/>
    ///     <see langword="true"/> when the pair was added; <see langword="false"/> when the key already exists.
    /// </returns>
    public bool TryAdd(KeyValuePair<TKey, TValue> item) => TryAdd(item.Key, item.Value);

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
    /// <remarks>
    ///     已有的键会被跳过——既不抛异常，也不出现在返回值和通知里；入参内部重复的键只有第一个能进得去。
    ///     一个都没加进去时不发通知。
    ///     <br/>
    ///     Keys that already exist are skipped: they neither throw nor show up in the return value or the
    ///     notification, and of duplicate keys inside the argument only the first one makes it in. When nothing
    ///     was added, no notification is raised.
    /// </remarks>
    public IReadOnlyDictionary<TKey, TValue> TryAddRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        List<KeyValuePair<TKey, TValue>> addedValues = [];
        foreach (var pair in values)
        {
            if (_innerDict.TryAdd(pair.Key, pair.Value)) addedValues.Add(pair);
        }

        if (addedValues.Count == 0) return ReadOnlyDictionary<TKey, TValue>.Empty;

        var dict = addedValues.ToImmutableDictionary();

        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        Readonly, dict
                    ));
        return dict;
    }

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
    public IReadOnlyDictionary<TKey, TValue> PutRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        List<KeyValuePair<TKey, TValue>> addedValues = [];
        List<KeyValuePair<TKey, TValue>> replacedOldValues = [];
        List<KeyValuePair<TKey, TValue>> replacedNewValues = [];
        foreach (var pair in values)
        {
            if (_innerDict.TryAdd(pair.Key, pair.Value)) addedValues.Add(pair);
            else
            {
                replacedOldValues.Add(new(pair.Key, _innerDict[pair.Key]));
                _innerDict[pair.Key] = pair.Value;
                replacedNewValues.Add(pair);
            }
        }

        IReadOnlyDictionary<TKey, TValue> result = ReadOnlyDictionary<TKey, TValue>.Empty;

        if (replacedNewValues.Count > 0)
        {
            var oldValues = replacedOldValues.ToImmutableDictionary();
            RaiseUpdate(new DictionaryItemReplacedEventArgs<TKey, TValue>(
                            Readonly,
                            oldValues,
                            replacedNewValues.ToImmutableDictionary()
                        ));
            result = oldValues;
        }
        if (addedValues.Count > 0)
        {
            RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                            Readonly,
                            addedValues.ToImmutableDictionary()
                        ));
        }

        return result;
    }

    /// <inheritdoc cref="Dictionary{TKey,TValue}.Remove(TKey, out TValue)" />
    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_innerDict.Remove(key, out value)) return false;
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
        return true;
    }

    /// <inheritdoc/>
    public bool Remove(TKey key) => Remove(key, out _);

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!((ICollection<KeyValuePair<TKey, TValue>>)_innerDict).Remove(item)) return false;
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { item.Key, item.Value } }.AsReadOnly()
                    ));
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
    public IReadOnlyDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
    {
        List<KeyValuePair<TKey, TValue>> removed = [];
        foreach (var key in keys)
        {
            if (!_innerDict.Remove(key, out var value)) continue;
            removed.Add(new(key, value));
        }

        if (removed.Count == 0) return ReadOnlyDictionary<TKey, TValue>.Empty;
        var dict = removed.ToImmutableDictionary();
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        Readonly, dict
                    ));
        return dict;
    }

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
    public IReadOnlyDictionary<TKey, TValue> Clear()
    {
        var before = _innerDict.ToImmutableDictionary();
        _innerDict.Clear();
        RaiseUpdate(new DictionaryItemClearedEventArgs<TKey, TValue>(Readonly, before));
        return before;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear() => Clear();

    #endregion


    // event

    private ImmutableList<EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>> _updatedEventHandlers = [];

    private void RaiseUpdate(DictionaryUpdateEventArgs<TKey, TValue> args)
    {
        foreach (var eventHandler in _updatedEventHandlers)
        {
            eventHandler.Invoke(this, args);
        }
    }

    /// <inheritdoc/>
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated
    {
        add
        {
            if (value is not null) _updatedEventHandlers = _updatedEventHandlers.Add(value);
        }
        remove
        {
            if (value is not null) _updatedEventHandlers = _updatedEventHandlers.Remove(value);
        }
    }
}