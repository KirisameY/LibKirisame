using KirisameY.BindingBridge.EventBinding;
using KirisameY.BindingBridge.EventBinding.Resolver;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     手写的事件源端点：按实例记录订阅到的处理程序，可以主动触发。
/// </summary>
/// <remarks>
///     库自带的 <c>DelegateEventNotifierEndpoint</c> 是 internal，所以测试只能从公开接口这一侧进入——
///     这恰好也覆盖了"消费端自己实现事件绑定策略"这条路径。
/// </remarks>
public sealed class ManualEventEndpoint<TObject, TDelegate> : IEventNotifierEndpoint<TObject, TDelegate>
    where TObject : class where TDelegate : Delegate
{
    private readonly List<(TObject Obj, TDelegate Handler)> _handlers = [];

    public void SubscribeEvent(TObject obj, TDelegate handler) => _handlers.Add((obj, handler));

    public void UnsubscribeEvent(TObject obj, TDelegate handler) =>
        _handlers.RemoveAll(entry => ReferenceEquals(entry.Obj, obj) && entry.Handler == handler);

    /// <summary>触发指定实例上的事件，把当前挂着的处理程序都调一遍。</summary>
    public void Raise(TObject obj, params object?[] args)
    {
        foreach (var (target, handler) in _handlers.ToArray())
        {
            if (ReferenceEquals(target, obj)) handler.DynamicInvoke(args);
        }
    }
}

/// <summary>事件目标端点：不管传进来的是哪个对象，永远交出同一个处理程序。</summary>
public sealed class FixedEventHandlerEndpoint<TObject, TDelegate>(TDelegate handler) : IEventHandlerEndpoint<TObject, TDelegate>
    where TDelegate : Delegate
{
    public TDelegate GetHandler(TObject obj) => handler;
}

/// <summary>只认指定的类型 + 事件名的源解析器，其余一律不认（返回 <see langword="null"/> 交给下一环）。</summary>
public sealed class FixedEventSourceResolver<TObject, TDelegate>(
    IEventNotifierEndpoint<TObject, TDelegate> endpoint, string eventName
) : IEventEndpointSourceResolver where TObject : class where TDelegate : Delegate
{
    public IEventNotifierEndpoint<T, TDel>? Resolve<T, TDel>(string name) where T : class where TDel : Delegate =>
        typeof(T) == typeof(TObject) && typeof(TDel) == typeof(TDelegate) && name == eventName
            ? (IEventNotifierEndpoint<T, TDel>)endpoint
            : null;
}
