using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace KirisameY.NotifiableCollections.Test.TestDoubles;

/// <summary>
///     <see cref="ObservableCollection{T}"/> 的子类：常规操作照旧走标准库实现（发出的通知形状与真实用法一致），
///     另外开放"先静默改内容、再手动发出任意通知"的能力，用来构造标准库自己产生不出来的形状
///     （比如一次移除多项、一次替换多项）。
/// </summary>
public class RaisableObservableCollection<T> : ObservableCollection<T>
{
    /// <summary>往集合里加元素但不发通知。</summary>
    public void AddSilently(T item) => Items.Add(item);

    /// <summary>按索引移除元素但不发通知。</summary>
    public void RemoveSilentlyAt(int index) => Items.RemoveAt(index);

    /// <summary>替换元素但不发通知。</summary>
    public void SetSilently(int index, T item) => Items[index] = item;

    /// <summary>手动发出一个通知。</summary>
    public void Raise(NotifyCollectionChangedEventArgs args) => OnCollectionChanged(args);
}
