using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     不实现 <see cref="System.ComponentModel.INotifyPropertyChanged"/> 的普通对象，
///     形状刻意与 <see cref="NotifyObject"/> 对齐，方便对照"有没有通知接口"带来的差异。
/// </summary>
public class PlainObject
{
    public int Number { get; set; }

    public string Text { get; set; } = "";

    public int Field;

    /// <summary>只读属性。</summary>
    public int ReadOnlyNumber => Number;

    private readonly Dictionary<string, int> _map = [];

    /// <summary>可读写索引器。</summary>
    public int this[string key]
    {
        get => _map.GetValueOrDefault(key);
        set => _map[key] = value;
    }
}

/// <summary>
///     完全不依赖 <see cref="System.ComponentModel.INotifyPropertyChanged"/> 的数据源：
///     自带一套事件，靠自定义解析器接进绑定。
/// </summary>
public class ManualNotifySource
{
    /// <summary>值变化时触发。</summary>
    public event Action? Changed;

    private int _number;

    public int Number
    {
        get => _number;
        set
        {
            if (_number == value) return;
            _number = value;
            Changed?.Invoke();
        }
    }

    /// <summary>为 <see cref="ManualNotifySource"/> 构造一个只认 <see cref="Number"/> 的解析器。</summary>
    public static IPropertyUpdateNotifyResolver CreateResolver() =>
        new PropertyUpdateNotifyResolverBuilder<ManualNotifySource>()
           .WithProperty(nameof(Number), (o, handler) => o.Changed += handler, (o, handler) => o.Changed -= handler)
           .Build();
}

/// <summary>承载静态属性的类型，用来验证静态成员表达式会被拒绝。</summary>
public static class StaticHolder
{
    public static int Value { get; set; }
}
