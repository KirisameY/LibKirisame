using System.Collections;
using System.Collections.Specialized;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     "是集合、但不是列表"的可观察数据源：只实现 <see cref="IReadOnlyCollection{T}"/> 与
///     <see cref="INotifyCollectionChanged"/>，用来走默认源解析器里 <c>NotifyCollectionObservableEndpoint</c>
///     那条分支——列表源走的是另一条。
/// </summary>
/// <remarks>
///     通知形状按标准库的约定发：Add / Remove 只带变更项、Reset 不带任何元素。
///     枚举顺序跟着 <see cref="HashSet{T}"/> 走，没有保证，断言里需要顺序时先排序。
/// </remarks>
public class ObservableSet<T> : IReadOnlyCollection<T>, INotifyCollectionChanged
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private readonly HashSet<T> _items = [];

    public int Count => _items.Count;

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Add(T item)
    {
        if (!_items.Add(item)) return false;
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        return true;
    }

    public bool Remove(T item)
    {
        if (!_items.Remove(item)) return false;
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        return true;
    }

    public void Clear()
    {
        if (_items.Count == 0) return;
        _items.Clear();
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void Raise(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);
}
