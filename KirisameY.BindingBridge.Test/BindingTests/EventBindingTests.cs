using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.EventBinding;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     事件绑定：<c>BindEvent(source, "EventName", target, t =&gt; t.Handler)</c>。
///     源侧要的是一个公开事件，目标侧要的是一个<b>委托类型的成员</b>，绑定负责把两者接起来、
///     并在 <c>Dispose</c> 时把处理程序摘下来。事件本身不参与"观察 / 写入"那套概念，只有转发与退订。
/// </summary>
public class EventBindingTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();

    [Fact]
    public void ARaisedEventReachesTheTargetHandler()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), target, t => t.Handler);

        source.Fire(7);

        Assert.Equal([7], target.Received);
    }

    [Fact]
    public void EveryRaiseIsForwarded()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), target, t => t.Handler);

        source.Fire(1);
        source.Fire(2);

        Assert.Equal([1, 2], target.Received);
    }

    [Fact]
    public void TheHandlerCanBeADelegatePropertyOnTheTarget()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), target, t => t.HandlerProperty);

        source.Fire(5);

        Assert.Equal([5], target.Received);
    }

    [Fact]
    public void AnEventWithoutParametersCanBeBoundToo()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(source, nameof(EventSource.Raised), target, t => t.RaiseHandler);

        source.Raise();

        Assert.Equal(1, target.RaiseCount);
    }

    [Fact]
    public void DisposingTheHandleUnsubscribesTheHandler()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        var handle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), target, t => t.Handler);
        source.Fire(1);
        Assert.Equal([1], target.Received);

        handle.Dispose();

        source.Fire(2);
        Assert.Equal([1], target.Received);
    }

    [Fact]
    public void DisposingTwiceIsHarmless()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        var handle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), target, t => t.Handler);

        handle.Dispose();
        handle.Dispose();

        source.Fire(1);
        Assert.Empty(target.Received);
    }

    [Fact]
    public void SeveralTargetsCanListenToTheSameEvent()
    {
        var source = new EventSource();
        var first = new EventRecorder();
        var second = new EventRecorder();

        using var firstHandle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), first, t => t.Handler);
        using var secondHandle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), second, t => t.Handler);

        source.Fire(3);

        Assert.Equal([3], first.Received);
        Assert.Equal([3], second.Received);
    }

    [Fact]
    public void AnEventDeclaredOnABaseTypeCanBeBoundThroughADerivedObject()
    {
        var source = new DerivedEventSource();
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(source, nameof(EventSource.Fired), target, t => t.Handler);

        source.Fire(4);

        Assert.Equal([4], target.Received);
    }

    [Fact]
    public void AnEventNameThatDoesNotExistIsRejected()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        var ex = Assert.Throws<ArgumentException>(
            () => DefaultBinder().BindEvent(source, "NoSuchEvent", target, t => t.Handler)
        );

        Assert.Contains("does not have observable event NoSuchEvent", ex.Message);
    }

    [Fact]
    public void ADelegateTypeTheEventCannotAcceptIsRejected()
    {
        var source = new EventSource();
        var target = new EventRecorder();

        // Fired 是 Action<int>，这里拿无参的 Action 去接：类型对不上，等同于"没有这个事件"
        var ex = Assert.Throws<ArgumentException>(
            () => DefaultBinder().BindEvent(source, nameof(EventSource.Fired), target, t => t.RaiseHandler)
        );

        Assert.Contains("does not have observable event Fired", ex.Message);
    }

    [Fact]
    public void ARegisteredResolverCanProvideTheEventEndpoint()
    {
        var source = new EventSource();
        var target = new EventRecorder();
        var endpoint = new ManualEventEndpoint<EventSource, Action<int>>();

        var binder = new DataBinderBuilder()
                    .WithEventSourceResolver(
                         typeof(EventSource),
                         new FixedEventSourceResolver<EventSource, Action<int>>(endpoint, nameof(EventSource.Fired))
                     )
                    .Build();

        using var handle = binder.BindEvent(source, nameof(EventSource.Fired), target, t => t.Handler);

        endpoint.Raise(source, 11);
        Assert.Equal([11], target.Received);

        // 真实的 Fired 没人订阅：投递确实走的自定义端点
        source.Fire(12);
        Assert.Equal([11], target.Received);
    }

    [Fact]
    public void TheFallbackResolverIsConsultedWhenNoTypeIsRegistered()
    {
        const string eventName = "NotAnEventAtAll";

        var source = new EventSource();
        var target = new EventRecorder();
        var endpoint = new ManualEventEndpoint<EventSource, Action<int>>();

        var binder = new DataBinderBuilder()
                    .WithEventSourceFallbackResolver(
                         new FixedEventSourceResolver<EventSource, Action<int>>(endpoint, eventName)
                     )
                    .Build();

        using var handle = binder.BindEvent(source, eventName, target, t => t.Handler);

        endpoint.Raise(source, 21);

        Assert.Equal([21], target.Received);
    }

    [Fact]
    public void AHandWrittenPairOfEndpointsCanBeBoundTogether()
    {
        var source = new EventSource();
        var target = new EventRecorder();
        var endpoint = new ManualEventEndpoint<EventSource, Action<int>>();
        var handlerEndpoint = new FixedEventHandlerEndpoint<EventRecorder, Action<int>>(target.Handler);

        var handle = endpoint.EventBindTo(handlerEndpoint, source, target);

        endpoint.Raise(source, 31);
        Assert.Equal([31], target.Received);

        handle.Dispose();

        endpoint.Raise(source, 32);
        Assert.Equal([31], target.Received);
    }
}
