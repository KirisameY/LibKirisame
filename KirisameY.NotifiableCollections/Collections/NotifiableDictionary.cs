using System.Collections;
using System.Collections.Immutable;

using KirisameY.NotifiableCollections.Collections.WrappedViews;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

public class NotifiableDictionary<TKey, TValue> : INotifiableDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _innerDict = [];

    private IReadOnlyDictionary<TKey, TValue> Readonly => field ??= _innerDict.AsReadOnly();


    #region Reading

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _innerDict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_innerDict).GetEnumerator();

    public bool ContainsKey(TKey key) => _innerDict.ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value) => _innerDict.TryGetValue(key, out value!);

    public int Count => _innerDict.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public IReadOnlyNotifiableCollection<TKey> Keys => field ??= new NotifiableDictionaryKeySet<TKey, TValue>(this);
    public IReadOnlyNotifiableCollection<TValue> Values => field ??= this.AsReadOnlyNotifiableCollection(p => p.Value);

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _innerDict.Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => _innerDict.Values;

    public bool Contains(KeyValuePair<TKey, TValue> item) => _innerDict.Contains(item);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerDict).CopyTo(array, arrayIndex);

    #endregion


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

    public void Add(TKey key, TValue value)
    {
        _innerDict.Add(key, value);
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public bool TryAdd(TKey key, TValue value)
    {
        if (!_innerDict.TryAdd(key, value)) return false;
        RaiseUpdate(new DictionaryItemAddedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
        return true;
    }

    public bool Remove(TKey key)
    {
        if (!_innerDict.Remove(key, out var value)) return false;
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { key, value } }.AsReadOnly()
                    ));
        return true;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!((ICollection<KeyValuePair<TKey, TValue>>)_innerDict).Remove(item)) return false;
        RaiseUpdate(new DictionaryItemRemovedEventArgs<TKey, TValue>(
                        Readonly,
                        new Dictionary<TKey, TValue> { { item.Key, item.Value } }.AsReadOnly()
                    ));
        return true;
    }

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