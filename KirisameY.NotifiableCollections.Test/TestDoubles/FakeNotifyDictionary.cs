using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace KirisameY.NotifiableCollections.Test.TestDoubles;

/// <summary>
///     标准库没有可观测字典，这里手写一个：读取照常，写操作按标准库的形状发出
///     <see cref="INotifyCollectionChanged"/> 通知，项一律是 <see cref="KeyValuePair{TKey,TValue}"/>。
/// </summary>
public class FakeNotifyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, INotifyCollectionChanged
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _items = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Count => _items.Count;
    public IEnumerable<TKey> Keys => _items.Keys;
    public IEnumerable<TValue> Values => _items.Values;
    public TValue this[TKey key] => _items[key];

    public bool ContainsKey(TKey key) => _items.ContainsKey(key);
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _items.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public FakeNotifyDictionary<TKey, TValue> With(TKey key, TValue value)
    {
        _items.Add(key, value);
        return this;
    }

    public void Add(TKey key, TValue value)
    {
        _items.Add(key, value);
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Pair(key, value)));
    }

    public void Remove(TKey key)
    {
        var removed = Pair(key, _items[key]);
        _items.Remove(key);
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
    }

    public void Set(TKey key, TValue value)
    {
        var old = Pair(key, _items[key]);
        _items[key] = value;
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Pair(key, value), old));
    }

    public void Clear()
    {
        _items.Clear();
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>改动内容但不发通知，供测试自行拼装通知形状。</summary>
    public void SetSilently(TKey key, TValue value) => _items[key] = value;

    /// <summary>手动发出一个通知。</summary>
    public void Raise(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);

    private static KeyValuePair<TKey, TValue> Pair(TKey key, TValue value) => new(key, value);
}
