using KirisameY.EventBus.Bus;

namespace KirisameY.EventBus.Test.BusTests;

public class UnsubscribeTests
{
    [Fact]
    public void ReturnsTrueWhenHandlerWasSubscribed()
    {
        var bus = new SimpleEventBus();
        Action<TestEvent> handler = _ => { };
        bus.Subscribe(handler);

        Assert.True(bus.Unsubscribe(handler));
    }

    [Fact]
    public void ReturnsFalseWhenHandlerWasNotSubscribed()
    {
        var bus = new SimpleEventBus();
        Action<TestEvent> handler = _ => { };
        bus.Subscribe<TestEvent>(_ => { });

        Assert.False(bus.Unsubscribe(handler));
    }

    [Fact]
    public void ReturnsFalseWhenEventTypeHasNoSubscription()
    {
        var bus = new SimpleEventBus();

        Assert.False(bus.Unsubscribe<TestEvent>(_ => { }));
    }

    [Fact]
    public void ReturnsFalseWhenRemovedTwice()
    {
        var bus = new SimpleEventBus();
        Action<TestEvent> handler = _ => { };
        bus.Subscribe(handler);

        Assert.True(bus.Unsubscribe(handler));
        Assert.False(bus.Unsubscribe(handler));
    }

    [Fact]
    public void HandlerStopsReceivingEventsAfterUnsubscribe()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> received = [];
        Action<TestEvent> handler = received.Add;
        bus.Subscribe(handler);

        bus.Publish(new TestEvent(1));
        Assert.Single(received);

        bus.Unsubscribe(handler);
        bus.Publish(new TestEvent(2));

        Assert.Single(received);
    }

    [Fact]
    public void OnlyMatchingHandlerIsRemoved()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> removed = [];
        List<TestEvent> kept = [];
        Action<TestEvent> removedHandler = removed.Add;
        bus.Subscribe(removedHandler);
        bus.Subscribe<TestEvent>(kept.Add);

        Assert.True(bus.Unsubscribe(removedHandler));
        bus.Publish(new TestEvent(1));

        Assert.Empty(removed);
        Assert.Single(kept);
    }

    [Fact]
    public void RemovingOneSubscriptionOfADuplicateHandlerKeepsTheOther()
    {
        var bus = new SimpleEventBus();
        var count = 0;
        Action<TestEvent> handler = _ => count++;
        bus.Subscribe(handler);
        bus.Subscribe(handler);

        Assert.True(bus.Unsubscribe(handler));
        bus.Publish(new TestEvent(1));

        // Unsubscribe 用的是 RemoveAll，同一处理器的多条订阅会被一次性摘干净
        Assert.Equal(0, count);
    }

    [Fact]
    public void DisposingTokenUnsubscribesHandler()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> received = [];
        Action<TestEvent> handler = received.Add;

        using (bus.Subscribe(handler))
        {
            bus.Publish(new TestEvent(1));
        }

        bus.Publish(new TestEvent(2));

        Assert.Single(received);
        Assert.Equal(1, received[0].Value);
    }

    [Fact]
    public void DisposingTokenTwiceDoesNotThrow()
    {
        var bus = new SimpleEventBus();
        var token = bus.Subscribe<TestEvent>(_ => { });

        token.Dispose();
        var exception = Record.Exception(token.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public void DisposingTokenDoesNotAffectOtherHandlers()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> kept = [];
        using (bus.Subscribe<TestEvent>(_ => { }))
        {
            bus.Subscribe<TestEvent>(kept.Add);
        }

        bus.Publish(new TestEvent(1));

        Assert.Single(kept);
    }
}
