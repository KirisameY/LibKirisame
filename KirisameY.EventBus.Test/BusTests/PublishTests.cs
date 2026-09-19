using KirisameY.EventBus.Bus;

namespace KirisameY.EventBus.Test.BusTests;

public class PublishTests
{
    [Fact]
    public void HandlersRunSynchronouslyInsidePublish()
    {
        var bus = new SimpleEventBus();
        List<string> trace = [];
        bus.Subscribe<TestEvent>(_ => trace.Add("handler"));

        trace.Add("before");
        bus.Publish(new TestEvent(1));
        trace.Add("after");

        // 没有队列兜着，Publish 返回时处理器已经跑完了
        Assert.Equal(["before", "handler", "after"], trace);
    }

    [Fact]
    public void PublishWithNoSubscribersDoesNotThrow()
    {
        var bus = new SimpleEventBus();

        var exception = Record.Exception(() => bus.Publish(new TestEvent(1)));

        Assert.Null(exception);
    }

    [Fact]
    public void PublishWithOnlyUnrelatedSubscribersDoesNotThrow()
    {
        var bus = new SimpleEventBus();
        bus.Subscribe<OtherEvent>(_ => { });

        var exception = Record.Exception(() => bus.Publish(new TestEvent(1)));

        Assert.Null(exception);
    }

    [Fact]
    public void HandlersAreInvokedInSubscriptionOrder()
    {
        var bus = new SimpleEventBus();
        List<string> order = [];
        bus.Subscribe<TestEvent>(_ => order.Add("first"));
        bus.Subscribe<TestEvent>(_ => order.Add("second"));
        bus.Subscribe<TestEvent>(_ => order.Add("third"));

        bus.Publish(new TestEvent(1));

        Assert.Equal(["first", "second", "third"], order);
    }

    [Fact]
    public void PublishesAreDeliveredInCallOrder()
    {
        var bus = new SimpleEventBus();
        List<int> values = [];
        bus.Subscribe<TestEvent>(e => values.Add(e.Value));

        bus.Publish(new TestEvent(1));
        bus.Publish(new TestEvent(2));
        bus.Publish(new TestEvent(3));

        Assert.Equal([1, 2, 3], values);
    }

    [Fact]
    public void DerivedHandlersRunBeforeBaseHandlers()
    {
        var bus = new SimpleEventBus();
        List<string> order = [];
        bus.Subscribe<TestEvent>(_ => order.Add("base"));
        bus.Subscribe<DerivedTestEvent>(_ => order.Add("derived"));

        bus.Publish(new DerivedTestEvent(1, "tag"));

        // 派发是从 typeof(TEvent) 沿 BaseType 往上走的，所以派生类型的处理器先跑
        Assert.Equal(["derived", "base"], order);
    }

    [Fact]
    public void BaseEventHandlerReceivesEveryEvent()
    {
        var bus = new SimpleEventBus();
        List<BaseEvent> received = [];
        bus.Subscribe<BaseEvent>(received.Add);

        bus.Publish(new TestEvent(1));
        bus.Publish(new OtherEvent("x"));
        bus.Publish(new DerivedTestEvent(2, "tag"));

        Assert.Equal(3, received.Count);
    }

    [Fact]
    public void PublishingThroughBaseStaticTypeOnlyReachesBaseHandlers()
    {
        var bus = new SimpleEventBus();
        List<string> order = [];
        bus.Subscribe<TestEvent>(_ => order.Add("base"));
        bus.Subscribe<DerivedTestEvent>(_ => order.Add("derived"));

        TestEvent @event = new DerivedTestEvent(1, "tag");
        bus.Publish(@event);

        // TEvent 由静态类型决定，只订阅了派生类型的处理器不会被运行时类型带出来
        Assert.Equal(["base"], order);
    }

    [Fact]
    public void HandlerExceptionPropagatesOutOfPublish()
    {
        var bus = new SimpleEventBus();
        bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("boom"));

        Assert.Throws<InvalidOperationException>(() => bus.Publish(new TestEvent(1)));
    }

    [Fact]
    public void HandlerExceptionStopsRemainingHandlers()
    {
        var bus = new SimpleEventBus();
        var laterHandlerRan = false;
        bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("boom"));
        bus.Subscribe<TestEvent>(_ => laterHandlerRan = true);

        Assert.Throws<InvalidOperationException>(() => bus.Publish(new TestEvent(1)));

        // 直接执行没有队列兜底，异常会当场中断本次派发
        Assert.False(laterHandlerRan);
    }

    [Fact]
    public void BusKeepsWorkingAfterHandlerThrows()
    {
        var bus = new SimpleEventBus();
        var shouldThrow = true;
        bus.Subscribe<TestEvent>(_ =>
        {
            if (shouldThrow) throw new InvalidOperationException("boom");
        });

        Assert.Throws<InvalidOperationException>(() => bus.Publish(new TestEvent(1)));

        shouldThrow = false;
        List<int> values = [];
        bus.Subscribe<TestEvent>(e => values.Add(e.Value));

        bus.Publish(new TestEvent(2));

        Assert.Equal([2], values);
    }

    [Fact]
    public void HandlerCanPublishNestedEvent()
    {
        var bus = new SimpleEventBus();
        List<string> order = [];
        bus.Subscribe<TestEvent>(_ =>
        {
            order.Add("outer");
            bus.Publish(new OtherEvent("nested"));
            order.Add("outer-resumed");
        });
        bus.Subscribe<OtherEvent>(_ => order.Add("nested"));

        bus.Publish(new TestEvent(1));

        // 嵌套发布同样是直接执行，内层跑完才回到外层处理器
        Assert.Equal(["outer", "nested", "outer-resumed"], order);
    }

    [Fact]
    public void HandlerUnsubscribingAnotherHandlerDoesNotAffectCurrentDispatch()
    {
        var bus = new SimpleEventBus();
        List<string> order = [];
        Action<TestEvent> second = _ => order.Add("second");
        bus.Subscribe<TestEvent>(_ =>
        {
            order.Add("first");
            bus.Unsubscribe(second);
        });
        bus.Subscribe(second);

        bus.Publish(new TestEvent(1));

        // 本次派发遍历的是订阅表快照，刚被摘掉的处理器仍然会被调用到
        Assert.Equal(["first", "second"], order);

        order.Clear();
        bus.Publish(new TestEvent(2));

        Assert.Equal(["first"], order);
    }

    [Fact]
    public void HandlerSubscribingDuringPublishIsNotCalledInCurrentDispatch()
    {
        var bus = new SimpleEventBus();
        List<string> order = [];
        var subscribed = false;
        bus.Subscribe<TestEvent>(_ =>
        {
            order.Add("first");
            if (subscribed) return;
            subscribed = true;
            bus.Subscribe<TestEvent>(_ => order.Add("late"));
        });

        bus.Publish(new TestEvent(1));

        // 本次派发用的是旧快照，新订阅者要等到下一次发布
        Assert.Equal(["first"], order);

        order.Clear();
        bus.Publish(new TestEvent(2));

        Assert.Equal(["first", "late"], order);
    }
}
