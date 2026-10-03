using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     链式属性绑定的载体：自己发 <see cref="INotifyPropertyChanged"/>，同时能挂下一级同类节点，
///     于是可以造出 <c>s =&gt; s.Inner.Inner.Number</c> 这种任意深度的属性链。
/// </summary>
/// <remarks>
///     <see cref="Inner"/> 可空，链的末端就是一个 <see cref="Inner"/> 为 <c>null</c> 的节点——
///     测试里写 <c>s =&gt; s.Inner!.Number</c>（<c>!</c> 在表达式树里会被抹掉，不影响取成员）。
/// </remarks>
public class ChainedNotifyNode : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _number;

    public int Number
    {
        get => _number;
        set
        {
            if (_number == value) return;
            _number = value;
            Raise(nameof(Number));
        }
    }

    private readonly Dictionary<string, int> _map = [];

    /// <summary>索引器：默认解析器把它映射成 <c>"Item[]"</c> 这个通知名。</summary>
    public int this[string key]
    {
        get => _map.GetValueOrDefault(key);
        set
        {
            if (_map.GetValueOrDefault(key) == value) return;
            _map[key] = value;
            Raise("Item[]");
        }
    }

    private ChainedNotifyNode? _inner;

    public ChainedNotifyNode? Inner
    {
        get => _inner;
        set
        {
            if (ReferenceEquals(_inner, value)) return;
            _inner = value;
            Raise(nameof(Inner));
        }
    }

    /// <summary>只读属性：链末端落在它身上时解析不出可写端点。</summary>
    public int ReadOnlyNumber => _number;

    /// <summary>手动发出指定名字的通知。</summary>
    public void Raise(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
///     不发任何通知的链根：它把 <see cref="ChainedNotifyNode"/> 挂在自己的属性上，
///     但属性变化既不通知、本身也不是 <see cref="INotifyPropertyChanged"/>，
///     于是链中间那一跳是"哑"的。
/// </summary>
public class SilentChainedRoot
{
    private ChainedNotifyNode _node = new();

    public ChainedNotifyNode Node
    {
        get => _node;
        set => _node = value;
    }
}

/// <summary>
///     链式集合绑定的载体：<see cref="Items"/> 整体替换时会发通知，用来观察
///     "链中间那一跳换了集合对象"之后绑定的去向。
/// </summary>
public class ChainedCollectionHolder : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<int> _items = [];

    public ObservableCollection<int> Items
    {
        get => _items;
        set
        {
            if (ReferenceEquals(_items, value)) return;
            _items = value;
            Raise(nameof(Items));
        }
    }

    private readonly List<int> _plainItems = [];

    /// <summary>普通 <see cref="List{T}"/> 且不发通知：集合本身不可观察，绑不上。</summary>
    public List<int> PlainItems => _plainItems;

    /// <summary>手动发出指定名字的通知。</summary>
    public void Raise(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
///     链上挂着的是"是集合、但不是列表"的可观察集合（<see cref="ObservableSet{T}"/>），
///     用来走链式那一侧的 <c>ChainedCollectionObservableEndpoint</c> 分支——
///     列表源走的是另一条。
/// </summary>
public class ChainedSetHolder : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableSet<int> _items = [];

    public ObservableSet<int> Items
    {
        get => _items;
        set
        {
            if (ReferenceEquals(_items, value)) return;
            _items = value;
            Raise(nameof(Items));
        }
    }

    /// <summary>手动发出指定名字的通知。</summary>
    public void Raise(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
