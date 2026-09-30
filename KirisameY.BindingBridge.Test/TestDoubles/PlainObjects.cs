using System.ComponentModel;

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

/// <summary>
///     一半靠自定义解析器、一半靠默认 <see cref="INotifyPropertyChanged"/> 路由的数据源：
///     <see cref="Number"/> 只会响自己的 <see cref="NumberChanged"/>、压根不发 PropertyChanged，
///     <see cref="Text"/> 才走 PropertyChanged。
/// </summary>
/// <remarks>
///     用它观察"同一个类型上两条路由各管一部分成员"：
///     把 <see cref="Number"/> 登记给类型级解析器，<see cref="Text"/> 就没人认领了，
///     接下来接手的是绑定器的兜底路由。
/// </remarks>
public class SplitNotifySource : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>值变化时触发；<see cref="Number"/> 只走这条路。</summary>
    public event Action? NumberChanged;

    private int _number;

    public int Number
    {
        get => _number;
        set
        {
            if (_number == value) return;
            _number = value;
            NumberChanged?.Invoke();
        }
    }

    private string _text = "";

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    }

    /// <summary>只认 <see cref="Number"/> 的解析器：把它接到 <see cref="NumberChanged"/> 上。</summary>
    public static IPropertyUpdateNotifyResolver NumberResolver() =>
        new PropertyUpdateNotifyResolverBuilder<SplitNotifySource>()
           .WithProperty(nameof(Number), (o, handler) => o.NumberChanged += handler, (o, handler) => o.NumberChanged -= handler)
           .Build();
}

/// <summary>承载静态属性的类型，用来验证静态成员表达式会被拒绝。</summary>
public static class StaticHolder
{
    public static int Value { get; set; }
}
