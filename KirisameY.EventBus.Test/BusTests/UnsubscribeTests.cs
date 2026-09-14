using KirisameY.EventBus.Bus;

namespace KirisameY.EventBus.Test.BusTests;

public class UnsubscribeTests
{
    [Fact]
    public void ReturnsTrueWhenHandlerWasSubscribed()
    {
        var bus = new AutoEventBus();
        Action<TestEvent> handler = _ => { };
        bus.Subscribe(handler);

        Assert.True(bus.Unsubscribe(handler));
    }

    [Fact]
    public void ReturnsFalseWhenHandlerWasNotSubscribed()
    {
        var bus = new AutoEventBus();
        Action<TestEvent> handler = _ => { };
        bus.Subscribe<TestEvent>(_ => { });

        Assert.False(bus.Unsubscribe(handler));
    }

    [Fact]
    public void ReturnsFalseWhenEventTypeHasNoSubscription()
    {
        var bus = new AutoEventBus();

        Assert.False(bus.Unsubscribe<TestEvent>(_ => { }));
    }

    [Fact]
    public void ReturnsFalseWhenRemovedTwice()
    {
        var bus = new AutoEventBus();
        Action<TestEvent> handler = _ => { };
        bus.Subscribe(handler);

        Assert.True(bus.Unsubscribe(handler));
        Assert.False(bus.Unsubscribe(handler));
    }

    [Fact]
    public void HandlerStopsReceivingEventsAfterUnsubscribe()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        Action<TestEvent> handler = received.Add;
        bus.Subscribe(handler);
        bus.Unsubscribe(handler);

        bus.OrderPost(new TestEvent(1)).Submit();

        Assert.Empty(received);
    }

    [Fact]
    public void OnlyMatchingHandlerIsRemoved()
    {
        var bus = new AutoEventBus();
        List<TestEvent> removed = [];
        List<TestEvent> kept = [];
        Action<TestEvent> removedHandler = removed.Add;
        bus.Subscribe(removedHandler);
        bus.Subscribe<TestEvent>(kept.Add);

        Assert.True(bus.Unsubscribe(removedHandler));
        bus.OrderPost(new TestEvent(1)).Submit();

        Assert.Empty(removed);
        Assert.Single(kept);
    }

    [Fact]
    public void DisposingTokenUnsubscribesHandler()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        Action<TestEvent> handler = received.Add;

        using (bus.Subscribe(handler))
        {
            bus.OrderPost(new TestEvent(1)).Submit();
        }

        bus.OrderPost(new TestEvent(2)).Submit();

        Assert.Single(received);
        Assert.Equal(1, received[0].Value);
    }

    [Fact]
    public void DisposingTokenTwiceDoesNotThrow()
    {
        var bus = new AutoEventBus();
        var token = bus.Subscribe<TestEvent>(_ => { });

        token.Dispose();
        var exception = Record.Exception(token.Dispose);

        Assert.Null(exception);
    }
}
