using KirisameY.BindingBridge.PropertyBinding;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     手写端点实现的公共部分：按实例记录订阅到的 handler，并统计订阅/退订调用次数。
/// </summary>
/// <remarks>
///     库自带的 <c>DelegateXxxPropertyEndpoint</c> 全是 internal，所以测试只能从公开接口这一侧进入。
///     这恰好也覆盖了"消费端自己实现绑定策略"这条路径。
/// </remarks>
public abstract class FakeEndpointBase<TObject>
{
    private readonly List<(TObject Obj, Action Handler)> _handlers = [];

    /// <summary>订阅调用总次数（同一实例重复订阅会重复计数）。</summary>
    public int SubscribeCalls { get; private set; }

    /// <summary>退订调用总次数（即使没有对应的订阅也会计数）。</summary>
    public int UnsubscribeCalls { get; private set; }

    /// <summary>当前挂在指定实例上的 handler 数量。</summary>
    public int HandlerCountOf(TObject obj) =>
        _handlers.Count(entry => EqualityComparer<TObject>.Default.Equals(entry.Obj, obj));

    /// <summary>触发指定实例上的更新通知；没有订阅者时静默。</summary>
    public void RaiseUpdate(TObject obj)
    {
        foreach (var entry in _handlers.ToArray())
        {
            if (EqualityComparer<TObject>.Default.Equals(entry.Obj, obj)) entry.Handler.Invoke();
        }
    }

    protected void OnSubscribe(TObject obj, Action handler)
    {
        SubscribeCalls++;
        _handlers.Add((obj, handler));
    }

    protected void OnUnsubscribe(TObject obj, Action handler)
    {
        UnsubscribeCalls++;
        var index = _handlers.FindIndex(entry =>
            EqualityComparer<TObject>.Default.Equals(entry.Obj, obj) && entry.Handler == handler
        );
        if (index >= 0) _handlers.RemoveAt(index);
    }
}

/// <summary>只读端点：只有取值能力。</summary>
public sealed class FakeReadOnlyEndpoint<TObject, TProperty>(Func<TObject, TProperty> getter) : IPropertyEndpoint<TObject, TProperty>
{
    public int GetCalls { get; private set; }

    public TProperty GetValue(TObject obj)
    {
        GetCalls++;
        return getter(obj);
    }
}

/// <summary>可观察端点：可取值、可订阅。</summary>
public sealed class FakePropertyObservableEndpoint<TObject, TProperty>(Func<TObject, TProperty> getter)
    : FakeEndpointBase<TObject>, IPropertyObservableEndpoint<TObject, TProperty>
{
    public int GetCalls { get; private set; }

    public TProperty GetValue(TObject obj)
    {
        GetCalls++;
        return getter(obj);
    }

    public void SubscribeUpdate(TObject obj, Action handler) => OnSubscribe(obj, handler);

    public void UnsubscribeUpdate(TObject obj, Action handler) => OnUnsubscribe(obj, handler);
}

/// <summary>可写端点：可取值、可写值，但不会发通知。</summary>
public sealed class FakePropertyWritableEndpoint<TObject, TProperty>(Func<TObject, TProperty> getter, Action<TObject, TProperty> setter)
    : FakeEndpointBase<TObject>, IPropertyWritableEndpoint<TObject, TProperty>
{
    public int GetCalls { get; private set; }
    public int SetCalls { get; private set; }

    /// <summary>按写入顺序记录下来的全部值。</summary>
    public List<TProperty> Written { get; } = [];

    public TProperty GetValue(TObject obj)
    {
        GetCalls++;
        return getter(obj);
    }

    public void SetValue(TObject obj, TProperty value)
    {
        SetCalls++;
        Written.Add(value);
        setter(obj, value);
    }
}

/// <summary>全能端点：四种接口齐备。</summary>
public sealed class FakePropertyUniversalEndpoint<TObject, TProperty>(Func<TObject, TProperty> getter, Action<TObject, TProperty> setter)
    : FakeEndpointBase<TObject>, IPropertyUniversalEndpoint<TObject, TProperty>
{
    /// <summary>
    ///     打开后，<see cref="SetValue"/> 引起的值变化会立刻回放一次通知，
    ///     用来模拟"行为良好的数据源"——值真的变了才发通知。
    /// </summary>
    public bool AutoNotify { get; set; }

    public int GetCalls { get; private set; }
    public int SetCalls { get; private set; }

    /// <summary>按写入顺序记录下来的全部值。</summary>
    public List<TProperty> Written { get; } = [];

    public TProperty GetValue(TObject obj)
    {
        GetCalls++;
        return getter(obj);
    }

    public void SetValue(TObject obj, TProperty value)
    {
        SetCalls++;
        Written.Add(value);

        var before = getter(obj);
        setter(obj, value);

        if (AutoNotify && !EqualityComparer<TProperty>.Default.Equals(before, value)) RaiseUpdate(obj);
    }

    public void SubscribeUpdate(TObject obj, Action handler) => OnSubscribe(obj, handler);

    public void UnsubscribeUpdate(TObject obj, Action handler) => OnUnsubscribe(obj, handler);
}
