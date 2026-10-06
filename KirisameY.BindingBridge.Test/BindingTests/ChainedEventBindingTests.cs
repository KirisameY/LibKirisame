using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     链式事件绑定（<c>BindEvent(obj, o =&gt; o.Notifier, "EventName", ...)</c>）：
///     事件挂在"从对象上取出来的另一个对象"上，那个属性被整体换掉时，绑定要跟着换到新对象上。
/// </summary>
public class ChainedEventBindingTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();

    [Fact]
    public void ANotifierOnAPropertyCanFeedTheTarget()
    {
        var source = new ChainedEventHolder();
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(
            source, s => s.Notifier, nameof(EventSource.Fired), target, t => t.Handler
        );

        source.Notifier.Fire(6);

        Assert.Equal([6], target.Received);
    }

    [Fact]
    public void ReplacingTheNotifierMovesTheBindingToTheNewOne()
    {
        var source = new ChainedEventHolder();
        var target = new EventRecorder();
        var oldNotifier = source.Notifier;

        using var handle = DefaultBinder().BindEvent(
            source, s => s.Notifier, nameof(EventSource.Fired), target, t => t.Handler
        );

        var newNotifier = new EventSource();
        source.Notifier = newNotifier;

        newNotifier.Fire(1);
        oldNotifier.Fire(2);

        Assert.Equal([1], target.Received);
    }

    [Fact]
    public void DisposingAfterReplacingTheNotifierUnsubscribesFromTheCurrentOne()
    {
        var source = new ChainedEventHolder();
        var target = new EventRecorder();

        var handle = DefaultBinder().BindEvent(
            source, s => s.Notifier, nameof(EventSource.Fired), target, t => t.Handler
        );

        var newNotifier = new EventSource();
        source.Notifier = newNotifier;
        newNotifier.Fire(1);
        Assert.Equal([1], target.Received);

        handle.Dispose();

        newNotifier.Fire(2);
        Assert.Equal([1], target.Received);
    }

    [Fact]
    public void DisposingAlsoStopsTheRebinding()
    {
        // 只把当前挂着的那个 notifier 摘干净还不够：链上"重接"的监听也得一起撤，
        // 否则之后再换 notifier 会把处理程序重新挂上去，绑定等于复活
        var source = new ChainedEventHolder();
        var target = new EventRecorder();

        var handle = DefaultBinder().BindEvent(
            source, s => s.Notifier, nameof(EventSource.Fired), target, t => t.Handler
        );
        handle.Dispose();

        var newNotifier = new EventSource();
        source.Notifier = newNotifier;
        newNotifier.Fire(1);

        Assert.Empty(target.Received);
    }

    [Fact]
    public void ANotifierPropertyThatNeverNotifiesKeepsTheBindingOnTheOriginalObject()
    {
        var source = new SilentEventRoot();
        var originalNotifier = source.Notifier;
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(
            source, s => s.Notifier, nameof(EventSource.Fired), target, t => t.Handler
        );

        originalNotifier.Fire(1);
        Assert.Equal([1], target.Received);

        // 中间那一跳不发通知，绑定就钉在旧对象上，换了也跟不过去
        var replacement = new EventSource();
        source.Notifier = replacement;
        replacement.Fire(2);

        Assert.Equal([1], target.Received);
    }

    [Fact]
    public void EveryNotifierThatWasReplacedIsAlsoUnsubscribedFrom()
    {
        var source = new ChainedEventHolder();
        var target = new EventRecorder();

        using var handle = DefaultBinder().BindEvent(
            source, s => s.Notifier, nameof(EventSource.Fired), target, t => t.Handler
        );

        var middleNotifier = new EventSource();
        source.Notifier = middleNotifier;
        var lastNotifier = new EventSource();
        source.Notifier = lastNotifier;

        lastNotifier.Fire(1);
        Assert.Equal([1], target.Received);

        // 已经被换下去的那个也不该再投递
        middleNotifier.Fire(2);
        Assert.Equal([1], target.Received);
    }

    [Fact]
    public void AnEventNameThatDoesNotExistOnTheNotifierIsRejected()
    {
        var source = new ChainedEventHolder();
        var target = new EventRecorder();

        var ex = Assert.Throws<ArgumentException>(
            () => DefaultBinder().BindEvent(source, s => s.Notifier, "NoSuchEvent", target, t => t.Handler)
        );

        Assert.Contains("does not have observable event NoSuchEvent", ex.Message);
    }
}
