using System.ComponentModel;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     事件绑定的源：只提供公开事件，<b>不</b>实现任何通知接口——事件绑定要的只是"有这么个事件"。
/// </summary>
/// <remarks>
///     <see cref="Fired"/> 带参、<see cref="Raised"/> 无参，覆盖两种委托形状。
/// </remarks>
public class EventSource
{
    /// <summary>带一个参数的事件。</summary>
    public event Action<int>? Fired;

    /// <summary>无参事件。</summary>
    public event Action? Raised;

    /// <summary>触发 <see cref="Fired"/>。</summary>
    public void Fire(int value) => Fired?.Invoke(value);

    /// <summary>触发 <see cref="Raised"/>。</summary>
    public void Raise() => Raised?.Invoke();
}

/// <summary>事件声明在基类上：从派生对象出发也能绑到它。</summary>
public class DerivedEventSource : EventSource;

/// <summary>
///     事件绑定的目标：把收到的值按顺序记下来。
/// </summary>
/// <remarks>
///     处理程序同时以字段（<see cref="Handler"/>）和只读属性（<see cref="HandlerProperty"/>）两种形状暴露，
///     两者交出的是<b>同一个</b>委托实例——绑定在挂上和摘下时会各取一次委托，取的必须是它。
/// </remarks>
public class EventRecorder
{
    /// <summary>按收到的顺序记录下来的值。</summary>
    public List<int> Received { get; } = [];

    /// <summary>挂到源事件上的处理程序。</summary>
    public Action<int> Handler;

    /// <summary>无参事件的处理程序，每被调用一次 <see cref="RaiseCount"/> 加一。</summary>
    public Action RaiseHandler;

    /// <summary><see cref="RaiseHandler"/> 被调用过的次数。</summary>
    public int RaiseCount { get; private set; }

    public EventRecorder()
    {
        Handler = Received.Add;
        RaiseHandler = () => RaiseCount++;
    }

    /// <summary>同一个委托实例，换成只读属性形状。</summary>
    public Action<int> HandlerProperty => Handler;
}

/// <summary>
///     链式事件绑定的载体：<see cref="Notifier"/> 整体替换时会发通知，
///     用来观察"链中间那一跳换了 notifier 对象"之后绑定的去向。
/// </summary>
public class ChainedEventHolder : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private EventSource _notifier = new();

    public EventSource Notifier
    {
        get => _notifier;
        set
        {
            if (ReferenceEquals(_notifier, value)) return;
            _notifier = value;
            Raise(nameof(Notifier));
        }
    }

    /// <summary>手动发出指定名字的通知。</summary>
    public void Raise(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
///     不发通知的链根：<see cref="Notifier"/> 换了也不通知、它本身也不是
///     <see cref="INotifyPropertyChanged"/>，于是链中间那一跳是"哑"的。
/// </summary>
public class SilentEventRoot
{
    public EventSource Notifier { get; set; } = new();
}
