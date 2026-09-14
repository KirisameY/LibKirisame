using KirisameY.EventBus.Bus;

namespace KirisameY.EventBus.Test.BusTests;

public class OrderPostTests
{
    [Fact]
    public void HandlersAreNotInvokedBeforeSubmit()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        bus.OrderPost(new TestEvent(1));

        Assert.Empty(received);
    }

    [Fact]
    public void HandlersAreInvokedOnSubmit()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        var order = bus.OrderPost(new TestEvent(1));
        order.Submit();

        Assert.Single(received);
        Assert.True(order.IsCompleted);
    }

    [Fact]
    public void OrderCompletesWithPostedEvent()
    {
        var bus = new AutoEventBus();
        var @event = new TestEvent(7);

        var order = bus.OrderPost(@event);
        order.Submit();

        Assert.True(order.IsCompleted);
        Assert.Same(@event, order.Result);
    }

    [Fact]
    public void OrderCompletesEvenWithNoHandlers()
    {
        var bus = new AutoEventBus();

        var order = bus.OrderPost(new TestEvent(1));
        order.Submit();

        Assert.True(order.IsCompleted);
    }

    [Fact]
    public void ContinueWithRunsAfterHandlers()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        TestEvent? continuedWith = null;
        var @event = new TestEvent(3);
        var order = bus.OrderPost(@event).ContinueWith(e => continuedWith = e);
        order.Submit();

        Assert.Single(received);
        Assert.Same(@event, continuedWith);
    }

    [Fact]
    public void MultiplePostsAreDeliveredInOrder()
    {
        var bus = new AutoEventBus();
        List<int> values = [];
        bus.Subscribe<TestEvent>(e => values.Add(e.Value));

        bus.OrderPost(new TestEvent(1)).Submit();
        bus.OrderPost(new TestEvent(2)).Submit();
        bus.OrderPost(new TestEvent(3)).Submit();

        Assert.Equal([1, 2, 3], values);
    }

    [Fact]
    public void HandlerExceptionPropagatesFromSubmit()
    {
        var bus = new AutoEventBus();
        bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("boom"));

        var order = bus.OrderPost(new TestEvent(1));

        Assert.Throws<InvalidOperationException>(() => order.Submit());
        Assert.False(order.IsCompleted);
    }

    private sealed class ManualEventBus : EventBusBase
    {
        public int EnqueueCount { get; private set; }

        protected override void PostEnqueued() => EnqueueCount++;

        public void Flush() => HandleQueue();
    }

    [Fact]
    public void HandlersWaitUntilQueueIsHandled()
    {
        var bus = new ManualEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        var order = bus.OrderPost(new TestEvent(1));
        order.Submit();

        Assert.Equal(1, bus.EnqueueCount);
        Assert.Empty(received);
        Assert.False(order.IsCompleted);

        bus.Flush();

        Assert.Single(received);
        Assert.True(order.IsCompleted);
    }

    [Fact]
    public void QueuedPostsAreHandledInOrder()
    {
        var bus = new ManualEventBus();
        List<int> values = [];
        bus.Subscribe<TestEvent>(e => values.Add(e.Value));

        bus.OrderPost(new TestEvent(1)).Submit();
        bus.OrderPost(new TestEvent(2)).Submit();

        Assert.Empty(values);

        bus.Flush();

        Assert.Equal([1, 2], values);
    }
}
