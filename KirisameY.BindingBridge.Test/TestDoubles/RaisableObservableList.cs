using System.Collections;
using System.Collections.Specialized;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     能一次带多项发通知的可观察列表源。
/// </summary>
/// <remarks>
///     <para>
///         标准库的 <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> 一次变更只带一项，
///         而 <see cref="NotifyCollectionChangedEventArgs"/> 本来就允许一次带多项
///         （构造函数直接收一个 <see cref="IList"/>，WPF 那套里批量增删改也是这么发的）。
///         要造出这种通知只能自己来。
///     </para>
///     <para>
///         三个批量方法都是"先真的改了自己的内容，再按标准格式发一条带多项的通知"，
///         所以从视图里读到的内容与通知里报的项是对得上的。
///     </para>
/// </remarks>
public class RaisableObservableList<T>(IEnumerable<T> items) : IReadOnlyList<T>, INotifyCollectionChanged
{
    private readonly List<T> _items = [..items];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Count => _items.Count;

    public T this[int index] => _items[index];

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>在 <paramref name="index"/> 处一次插入多项。</summary>
    public void InsertRange(int index, IEnumerable<T> added)
    {
        var newItems = added.ToList();
        _items.InsertRange(index, newItems);
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, index));
    }

    /// <summary>从 <paramref name="index"/> 起一次移除 <paramref name="count"/> 项。</summary>
    public void RemoveRange(int index, int count)
    {
        var oldItems = _items.GetRange(index, count);
        _items.RemoveRange(index, count);
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, index));
    }

    /// <summary>从 <paramref name="index"/> 起一次换掉连续若干项。</summary>
    public void ReplaceRange(int index, IEnumerable<T> newItems)
    {
        var replacements = newItems.ToList();
        var oldItems = _items.GetRange(index, replacements.Count);
        for (var i = 0; i < replacements.Count; i++) _items[index + i] = replacements[i];
        Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replacements, oldItems, index));
    }

    private void Raise(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);
}
