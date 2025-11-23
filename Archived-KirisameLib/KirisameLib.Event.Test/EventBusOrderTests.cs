using KirisameLib.Extensions;

namespace KirisameLib.Event.Test;

public class EventBusOrderTests
{
    private abstract record TestEvent(string Message) : BaseEvent;

    private record SendEvent(string Message) : TestEvent(Message);

    private record EchoEvent(string Message, int Time = 0) : TestEvent(Message);

    private static void AwaitOrderTest(EventBus bus)
    {
        List<string> received = [];

        bus.Subscribe<TestEvent>(e => Console.WriteLine(e.Message));
        bus.Subscribe<TestEvent>(e => received.Add(e.Message));
        bus.Subscribe<TestEvent>(e => Console.WriteLine($"once: {e.Message}"), HandlerSubscribeFlag.OnlyOnce);
        bus.Subscribe<TestEvent>(e => received.Add($"once: {e.Message}"),      HandlerSubscribeFlag.OnlyOnce);
        bus.Subscribe<SendEvent>(e => bus.Publish(new EchoEvent(GetEcho(e.Message))));

        bus.Publish(new SendEvent("Hello World!"));

        var task = ((Func<Task>)(async () =>
        {
            await bus.PublishAndWaitFor(new SendEvent("And I send this first")).ConfigureAwait(false);
            bus.Publish(new SendEvent("and this 2nd"));
            received.Add("and put this stealthily 3rd");
            Console.WriteLine("and put this stealthily 3rd");
        })).Invoke();

        bus.Publish(new SendEvent("Now I'm done"));

        if (bus is DelayedEventBus delayedBus) delayedBus.HandleEvent(); //todo: 炸了>_<

        task.Wait();

        string[] expected =
        [
            "Hello World!",
            "once: Hello World!",
            GetEcho("Hello World!"),
            "And I send this first",
            "and put this stealthily 3rd",
            GetEcho("And I send this first"),
            "and this 2nd",
            GetEcho("and this 2nd"),
            "Now I'm done",
            GetEcho("Now I'm done"),
        ];
        Assert.That(received.Join('\n'), Is.EqualTo(expected.Join('\n')));

        return;

        string GetEcho(string msg) => $"- I got a msg: \"{msg}\"!";
    }

    [Test] public void ImmediateAwaitOrderTest() => AwaitOrderTest(new ImmediateEventBus((_, ex) => Console.WriteLine(ex)));

    [Test] public void DelayedAwaitOrderTest() => AwaitOrderTest(new DelayedEventBus((_, ex) => Console.WriteLine(ex)));

    private static void ChainedOrderTest(EventBus bus)
    {
        List<string> received1 = [];
        List<string> received2 = [];

        bus.Subscribe<TestEvent>(e => Console.WriteLine(e.Message));
        bus.Subscribe<TestEvent>(e => received1.Add(e.Message));
        bus.Subscribe<SendEvent>(e => bus.Publish(new EchoEvent($"{e.Message}: 0")));
        bus.Subscribe<EchoEvent>(e =>
        {
            if (e.Time <= 5)
                bus.Publish(new EchoEvent($"{e.Message}: {e.Time + 1}", e.Time + 1));
        });
        bus.Subscribe<TestEvent>(e => received2.Add(e.Message));
        bus.Subscribe<EchoEvent>(e =>
        {
            if (e.Time == 2)
                bus.Publish(new EchoEvent($"Got 2: {e.Message}", Time: 6));
        });

        bus.Publish(new SendEvent("Hello World!"));
        bus.Publish(new SendEvent("Bye World!"));

        if (bus is DelayedEventBus delayedBus) delayedBus.HandleEvent();

        string[] expected =
        [
            "Hello World!",
            "Hello World!: 0",
            "Hello World!: 0: 1",
            "Hello World!: 0: 1: 2",
            "Hello World!: 0: 1: 2: 3",
            "Got 2: Hello World!: 0: 1: 2",
            "Hello World!: 0: 1: 2: 3: 4",
            "Hello World!: 0: 1: 2: 3: 4: 5",
            "Hello World!: 0: 1: 2: 3: 4: 5: 6",
            "Bye World!",
            "Bye World!: 0",
            "Bye World!: 0: 1",
            "Bye World!: 0: 1: 2",
            "Bye World!: 0: 1: 2: 3",
            "Got 2: Bye World!: 0: 1: 2",
            "Bye World!: 0: 1: 2: 3: 4",
            "Bye World!: 0: 1: 2: 3: 4: 5",
            "Bye World!: 0: 1: 2: 3: 4: 5: 6",
        ];

        Assert.Multiple(() =>
        {
            Assert.That(received1.Join('\n'), Is.EqualTo(expected.Join('\n')));
            Assert.That(received2.Join('\n'), Is.EqualTo(expected.Join('\n')));
        });
    }

    [Test] public void ImmediateChainedOrderTest() => ChainedOrderTest(new ImmediateEventBus((_, ex) => Console.WriteLine(ex)));

    [Test] public void DelayedChainedOrderTest() => ChainedOrderTest(new DelayedEventBus((_, ex) => Console.WriteLine(ex)));
}