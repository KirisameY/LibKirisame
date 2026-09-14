using KirisameY.EventBus.Bus;

namespace KirisameY.EventBus.Test.BusTests;

public class SubscribeTests
{
    [Fact]
    public void HandlerReceivesPostedEvent()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        var @event = new TestEvent(42);
        bus.OrderPost(@event).Submit();

        Assert.Single(received);
        Assert.Same(@event, received[0]);
    }

    [Fact]
    public void MultipleHandlersAllReceiveEvent()
    {
        var bus = new AutoEventBus();
        List<TestEvent> first = [];
        List<TestEvent> second = [];
        bus.Subscribe<TestEvent>(first.Add);
        bus.Subscribe<TestEvent>(second.Add);

        bus.OrderPost(new TestEvent(1)).Submit();

        Assert.Single(first);
        Assert.Single(second);
    }

    [Fact]
    public void SameHandlerSubscribedTwiceIsInvokedTwice()
    {
        var bus = new AutoEventBus();
        var count = 0;
        Action<TestEvent> handler = _ => count++;
        bus.Subscribe(handler);
        bus.Subscribe(handler);

        bus.OrderPost(new TestEvent(1)).Submit();

        Assert.Equal(2, count);
    }

    [Fact]
    public void HandlersAreIsolatedPerEventType()
    {
        var bus = new AutoEventBus();
        List<TestEvent> testEvents = [];
        List<OtherEvent> otherEvents = [];
        bus.Subscribe<TestEvent>(testEvents.Add);
        bus.Subscribe<OtherEvent>(otherEvents.Add);

        bus.OrderPost(new TestEvent(1)).Submit();

        Assert.Single(testEvents);
        Assert.Empty(otherEvents);
    }

    [Fact]
    public void BaseTypeHandlerReceivesDerivedEvent()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        var @event = new DerivedTestEvent(1, "tag");
        bus.OrderPost(@event).Submit();

        Assert.Single(received);
        Assert.Same(@event, received[0]);
    }

    [Fact]
    public void BothBaseAndDerivedHandlersReceiveDerivedEvent()
    {
        var bus = new AutoEventBus();
        List<TestEvent> baseReceived = [];
        List<DerivedTestEvent> derivedReceived = [];
        bus.Subscribe<TestEvent>(baseReceived.Add);
        bus.Subscribe<DerivedTestEvent>(derivedReceived.Add);

        bus.OrderPost(new DerivedTestEvent(1, "tag")).Submit();

        Assert.Single(baseReceived);
        Assert.Single(derivedReceived);
    }

    [Fact]
    public void ReceivedEventIsSingleInstanceWithoutDuplicateDispatch()
    {
        var bus = new AutoEventBus();
        List<TestEvent> received = [];
        bus.Subscribe<TestEvent>(received.Add);

        bus.OrderPost(new TestEvent(1)).Submit();

        Assert.Single(received);
    }

    [Fact]
    public void UnrelatedEventTypeHandlerIsNotInvoked()
    {
        var bus = new AutoEventBus();
        var invoked = false;
        bus.Subscribe<OtherEvent>(_ => invoked = true);

        bus.OrderPost(new TestEvent(1)).Submit();

        Assert.False(invoked);
    }
}
