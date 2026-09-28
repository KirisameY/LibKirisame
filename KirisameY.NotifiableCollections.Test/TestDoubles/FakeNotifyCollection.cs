using System.Collections;
using System.Collections.Specialized;

namespace KirisameY.NotifiableCollections.Test.TestDoubles;

/// <summary>
///     只实现 <see cref="IReadOnlyCollection{T}"/> 的标准库风格可观测集合——<b>不是</b>列表。
///     用来确认集合版包装确实只要求「能数、能枚举」，不依赖索引。
/// </summary>
public class FakeNotifyCollection<T> : IReadOnlyCollection<T>, INotifyCollectionChanged
{
    private readonly List<T> _items = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Count => _items.Count;

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>加入一个元素，并按标准库的形状发出一条单项 Add 通知。</summary>
    public void Add(T item)
    {
        _items.Add(item);
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    /// <summary>清空并发出 Reset 通知。</summary>
    public void Clear()
    {
        _items.Clear();
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>手动发出一个通知。</summary>
    public void Raise(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);
}
