using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.Collections.WrappedViews;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     用于简易实现 <see cref="INotifiableDictionary{TKey,TValue}"/> 的基类型。
///     <br/>
///     Base type for implement <see cref="INotifiableDictionary{TKey,TValue}"/> easily.
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
public abstract class NotifiableDictionarySimpleBase<TKey, TValue> : INotifiableDictionary<TKey, TValue> where TKey : notnull
{
    #region Abstract & Virtual

    /// <summary>
    ///     该字典的只读包装视图。
    ///     <br/>
    ///     Read-only wrapped view of this dictionary.
    /// </summary>
    protected abstract IReadOnlyDictionary<TKey, TValue> View { get; }

    /// <summary>
    ///     为字典设定一个键对应的值，添加或替换。
    ///     <br/>
    ///     Set a value for a key in the dictionary, by add or replace.
    /// </summary>
    protected abstract void SetValue(TKey key, TValue value);

    /// <summary>
    ///     尝试向字典中添加指定的键值对。
    ///     <br/>
    ///     Try to add given key-value pair to the dictionary.
    /// </summary>
    /// <returns>
    ///     是否该键先前未存在于字典中并已被成功添加。
    ///     <br/>
    ///     Whether the key was not exists in the dictionary, and has already been added.
    /// </returns>
    protected virtual bool TryAddValue(TKey key, TValue value)
    {
        if (ContainsKey(key)) return false;
        SetValue(key, value);
        return true;
    }

    /// <summary>
    ///     向字典中添加指定的键值对。
    ///     <br/>
    ///     Add given key-value pair to the dictionary.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     键已存在于字典中。
    ///     <br/>
    ///     The given key is already exists in the dictionary.
    /// </exception>
    protected virtual void AddValue(TKey key, TValue value)
    {
        if (!TryAddValue(key, value)) throw new ArgumentException("An element with the same key already exists");
    }

    /// <summary>
    ///     从字典中移除指定的键，并将被移除的值通过 <paramref name="value"/> 传出。
    ///     <br/>
    ///     Remove specified key from the dictionary, and out put its value via <paramref name="value"/>.
    /// </summary>
    /// <returns>
    ///     是否成功找到并移除。
    ///     <br/>
    ///     Whether found and removed successfully.
    /// </returns>
    protected abstract bool RemoveValue(TKey key, [MaybeNullWhen(false)] out TValue value);

    /// <summary>
    ///     从字典中移除指定的键。
    ///     <br/>
    ///     Remove specified key from the dictionary.
    /// </summary>
    /// <returns>
    ///     是否成功找到并移除。
    ///     <br/>
    ///     Whether found and removed successfully.
    /// </returns>
    protected virtual bool RemoveValue(TKey key) => RemoveValue(key, out _);

    /// <summary>
    ///     移除字典里的所有键值对。
    ///     <br/>
    ///     Remove all key-value pairs in the dictionary.
    /// </summary>
    protected virtual void ClearValues()
    {
        foreach (var key in Keys.ToArray())
        {
            RemoveValue(key);
        }
    }

    #endregion


    #region Reading

    /// <inheritdoc cref="INotifiableDictionary{TKey,TValue}.Count"/>
    public int Count => View.Count;

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => View.GetEnumerator();

    /// <inheritdoc cref="INotifiableDictionary{TKey,TValue}.ContainsKey"/>
    public bool ContainsKey(TKey key) => View.ContainsKey(key);

    /// <inheritdoc cref="INotifiableDictionary{TKey,TValue}.TryGetValue"/>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => View.TryGetValue(key, out value);

    /// <inheritdoc />
    public IReadOnlyNotifiableCollection<TKey> Keys => field ??= new NotifiableDictionaryKeySet<TKey, TValue>(this);

    /// <inheritdoc />
    public IReadOnlyNotifiableCollection<TValue> Values => field ??= this.AsReadOnlyNotifiableCollection(p => p.Value);

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => field ??= new KeyCollection(this);
    ICollection<TValue> IDictionary<TKey, TValue>.Values => field ??= new ValueCollection(this);

    #endregion


    #region Writing

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.this"/>
    public TValue this[TKey key]
    {
        get => View[key];
        set
        {
            if (View.TryGetValue(key, out var oldValue))
            {
                SetValue(key, value);
                RaiseUpdate(new DictionaryItemReplacedEventArgs<TKey, TValue>(
                                View,
                                new Dictionary<TKey, TValue> { { key, oldValue } }.AsReadOnly(),
                                new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                            ));
            }
            else
            {
                SetValue(key, value);
                RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                                View,
                                new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                            ));
            }
        }
    }

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
        AddValue(key, value);
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        View,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
    }


    /// <inheritdoc/>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        var dict = values.ToImmutableDictionary();
        if (dict.IsEmpty) return;

        var items = dict.ToArray();
        foreach (var (index, pair) in items.Index())
        {
            try
            {
                AddValue(pair.Key, pair.Value);
            }
            catch (ArgumentException)
            {
                for (int i = 0; i < index; i++) RemoveValue(items[i].Key);
                throw;
            }
        }
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        View, dict
                    ));
    }

    /// <inheritdoc/>
    public bool TryAdd(TKey key, TValue value)
    {
        if (!TryAddValue(key, value)) return false;
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        View,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<TKey, TValue> TryAddRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        List<KeyValuePair<TKey, TValue>> addedValues = [];
        foreach (var pair in values)
        {
            if (TryAddValue(pair.Key, pair.Value)) addedValues.Add(pair);
        }

        if (addedValues.Count == 0) return ReadOnlyDictionary<TKey, TValue>.Empty;

        var dict = addedValues.ToImmutableDictionary();

        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        View, dict
                    ));
        return dict;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<TKey, TValue> PutRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        List<KeyValuePair<TKey, TValue>> addedValues = [];
        List<KeyValuePair<TKey, TValue>> replacedOldValues = [];
        List<KeyValuePair<TKey, TValue>> replacedNewValues = [];
        foreach (var pair in values)
        {
            if (TryAddValue(pair.Key, pair.Value)) addedValues.Add(pair);
            else
            {
                replacedOldValues.Add(new(pair.Key, View[pair.Key]));
                SetValue(pair.Key, pair.Value);
                replacedNewValues.Add(pair);
            }
        }

        IReadOnlyDictionary<TKey, TValue> result = ReadOnlyDictionary<TKey, TValue>.Empty;

        if (replacedNewValues.Count > 0)
        {
            var oldValues = replacedOldValues.ToImmutableDictionary();
            RaiseUpdate(new DictionaryItemReplacedEventArgs<TKey, TValue>(
                            View,
                            oldValues,
                            replacedNewValues.ToImmutableDictionary()
                        ));
            result = oldValues;
        }
        if (addedValues.Count > 0)
        {
            RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                            View,
                            addedValues.ToImmutableDictionary()
                        ));
        }

        return result;
    }

    /// <inheritdoc/>
    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!RemoveValue(key, out value)) return false;
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        View,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
    {
        List<KeyValuePair<TKey, TValue>> removed = [];
        foreach (var key in keys)
        {
            if (!RemoveValue(key, out var value)) continue;
            removed.Add(new(key, value));
        }

        if (removed.Count == 0) return ReadOnlyDictionary<TKey, TValue>.Empty;
        var dict = removed.ToImmutableDictionary();
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        View, dict
                    ));
        return dict;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<KeyValuePair<TKey, TValue>> RemoveAll(Predicate<KeyValuePair<TKey, TValue>> predicate)
    {
        var result = View.Where(predicate.Invoke).ToImmutableDictionary();
        foreach (var (key, _) in result) RemoveValue(key);
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(View, result));
        return result;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<TKey, TValue> Clear()
    {
        var before = View.ToImmutableDictionary();
        ClearValues();
        RaiseUpdate(new DictionaryItemClearedEventArgs<TKey, TValue>(View, before));
        return before;
    }

    #endregion


    #region Event

    private ImmutableList<EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>> _updatedEventHandlers = [];

    private void RaiseUpdate(DictionaryUpdateEventArgs<TKey, TValue> args)
    {
        foreach (var eventHandler in _updatedEventHandlers)
        {
            eventHandler.Invoke(this, args);
        }
    }

    /// <inheritdoc/>
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>> DictionaryUpdated
    {
        add => _updatedEventHandlers = _updatedEventHandlers.Add(value);
        remove => _updatedEventHandlers = _updatedEventHandlers.Remove(value);
    }

    #endregion


    #region K&V Collection Implements

    private class KeyCollection(NotifiableDictionarySimpleBase<TKey, TValue> dict) : ICollection<TKey>
    {
        public IEnumerator<TKey> GetEnumerator() => dict.Select(p => p.Key).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => dict.Count;

        public bool Contains(TKey item) => dict.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            foreach (var (i, key) in this.Index()) array[arrayIndex + i] = key;
        }

        public bool IsReadOnly => true;

        public void Add(TKey item) => throw new NotSupportedException();

        public bool Remove(TKey item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();
    }

    private class ValueCollection(NotifiableDictionarySimpleBase<TKey, TValue> dict) : ICollection<TValue>
    {
        public IEnumerator<TValue> GetEnumerator() => dict.Select(p => p.Value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => dict.Count;

        public bool Contains(TValue item) => this.Any(v => EqualityComparer<TValue>.Default.Equals(item, v));

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            foreach (var (i, key) in this.Index()) array[arrayIndex + i] = key;
        }

        public bool IsReadOnly => true;

        public void Add(TValue item) => throw new NotSupportedException();

        public bool Remove(TValue item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();
    }

    #endregion
}