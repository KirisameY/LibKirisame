using KirisameY.EventBus.Bus;

namespace KirisameY.EventBus.Test.BusTests;

public class SubscribeTests
{
    [Fact]
    public void HandlerReceivesPublishedEvent()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        var @event = new TestEvent(42);
        bus.Publish(@event);

        Assert.Single(received);
        Assert.Same(@event, received[0]);
    }

    [Fact]
    public void MultipleHandlersAllReceiveEvent()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> first = [];
        List<TestEvent> second = [];
        bus.Subscribe<TestEvent>(first.Add);
        bus.Subscribe<TestEvent>(second.Add);

        bus.Publish(new TestEvent(1));

        Assert.Single(first);
        Assert.Single(second);
    }

    [Fact]
    public void SameHandlerSubscribedTwiceIsInvokedTwice()
    {
        var bus = new SimpleEventBus();
        var count = 0;
        Action<TestEvent> handler = _ => count++;
        bus.Subscribe(handler);
        bus.Subscribe(handler);

        bus.Publish(new TestEvent(1));

        Assert.Equal(2, count);
    }

    [Fact]
    public void HandlersAreIsolatedPerEventType()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> testEvents = [];
        List<OtherEvent> otherEvents = [];
        bus.Subscribe<TestEvent>(testEvents.Add);
        bus.Subscribe<OtherEvent>(otherEvents.Add);

        bus.Publish(new TestEvent(1));

        Assert.Single(testEvents);
        Assert.Empty(otherEvents);
    }

    [Fact]
    public void BaseTypeHandlerReceivesDerivedEvent()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        var @event = new DerivedTestEvent(1, "tag");
        bus.Publish(@event);

        Assert.Single(received);
        Assert.Same(@event, received[0]);
    }

    [Fact]
    public void BothBaseAndDerivedHandlersReceiveDerivedEvent()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> baseReceived = [];
        List<DerivedTestEvent> derivedReceived = [];
        bus.Subscribe<TestEvent>(baseReceived.Add);
        bus.Subscribe<DerivedTestEvent>(derivedReceived.Add);

        bus.Publish(new DerivedTestEvent(1, "tag"));

        Assert.Single(baseReceived);
        Assert.Single(derivedReceived);
    }

    [Fact]
    public void ReceivedEventIsSingleInstanceWithoutDuplicateDispatch()
    {
        var bus = new SimpleEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        bus.Publish(new TestEvent(1));

        Assert.Single(received);
    }

    [Fact]
    public void UnrelatedEventTypeHandlerIsNotInvoked()
    {
        var bus = new SimpleEventBus();
        var invoked = false;
        bus.Subscribe<OtherEvent>(_ => invoked = true);

        bus.Publish(new TestEvent(1));

        Assert.False(invoked);
    }
}
