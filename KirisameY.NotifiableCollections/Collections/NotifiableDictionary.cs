using System.Collections;
using System.Collections.Immutable;

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
///         变更以键值对为单位报告（见 <see cref="IDictionaryUpdateEventArgs{TKey, TValue}"/> 一族），
///         且移除通知里带的是键被移除<b>时</b>的值。
///         <see cref="Keys"/> 与 <see cref="Values"/> 本身就是可通知的视图，可以只订阅其中一边。
///         <br/>
///         通知是在内部字典<b>改完之后</b>同步发出的，事件参数里携带的视图是活视图而非快照。
///     </para>
///     <para>
///         Changes are reported in terms of key-value pairs (see the
///         <see cref="IDictionaryUpdateEventArgs{TKey, TValue}"/> family), and a removal notification carries the
///         values the keys held <b>at the time</b> they were removed. <see cref="Keys"/> and
///         <see cref="Values"/> are notifying views in their own right, so either side can be subscribed to
///         independently.
///         <br/>
///         Notifications are raised synchronously <b>after</b> the underlying dictionary has been modified, and
///         the views carried by the event arguments are live views rather than snapshots.
///     </para>
/// </remarks>
public class NotifiableDictionary<TKey, TValue> : INotifiableDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _innerDict = [];

    private IReadOnlyDictionary<TKey, TValue> Readonly => field ??= _innerDict.AsReadOnly();


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

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
        _innerDict.Add(key, value);
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

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

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
        if (!_innerDict.Remove(key, out var value)) return false;
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
        return true;
    }

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

    /// <inheritdoc/>
    public void Clear()
    {
        var before = _innerDict.ToImmutableDictionary();
        _innerDict.Clear();
        RaiseUpdate(new DictionaryItemClearedEventArgs<TKey, TValue>(Readonly, before));
    }


    private readonly List<EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>> _dictionaryUpdatedEventHandlers = [];

    private void RaiseUpdate(DictionaryUpdateEventArgs<TKey, TValue> args)
    {
        foreach (var eventHandler in _dictionaryUpdatedEventHandlers)
        {
            eventHandler.Invoke(this, args);
        }
    }

    /// <inheritdoc/>
    public event EventHandler<DictionaryUpdateEventArgs<TKey, TValue>>? DictionaryUpdated
    {
        add
        {
            if (value is not null) _dictionaryUpdatedEventHandlers.Add(value);
        }
        remove
        {
            if (value is not null) _dictionaryUpdatedEventHandlers.Remove(value);
        }
    }
}