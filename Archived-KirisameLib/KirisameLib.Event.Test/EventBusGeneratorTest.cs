using KirisameLib.Asynchronous.SyncTasking;

namespace KirisameLib.Event.Test;

public class EventBusGeneratorTest
{
    [Test]
    public void InstanceTest()
    {
        EventBus bus = new ImmediateEventBus((_, ex) => Console.WriteLine(ex));
        List<string> baseReceived = [];
        List<string> inheritedReceived = [];

        var baseClass = new TestBaseClass("base", e => baseReceived.Add(e));
        var inheritedClass = new TestInheritedClass("inherited", e => inheritedReceived.Add(e));

        baseClass.Register(bus);
        inheritedClass.Register(bus);

        bus.Publish(new TestEvent("Hello World!"));
        bus.Publish(new TestEvent("That's 2nd msg"));
        bus.Publish(new AnotherTestEvent("3rd here, don't tell him"));

        inheritedClass.RegisterTestA(bus);

        bus.Publish(new TestEvent("Bye World!"));

        string[] baseExpected =
        [
            "Hello World!",
            "That's 2nd msg",
            "Bye World!"
        ];
        string[] inheritedExpected =
        [
            "Hello World!",
            "That's 2nd msg",
            "3rd here, don't tell him",
            "Bye World!",
            "TestA:Bye World!",
            "Bye World!",
        ];

        Assert.Multiple(() =>
        {
            Assert.That(baseReceived, Is.EquivalentTo(baseExpected));
            Assert.That(inheritedReceived, Is.EquivalentTo(inheritedExpected));
        });
    }
}

[EventHandlerContainer]
public partial class TestBaseClass(string name, Action<string> handler)
{
    protected string Name { get; } = name;

    [EventHandler(["", "testA"])]
    private void TestEventHandler(TestEvent e)
    {
        handler.Invoke(e.Message);
        Console.Write($"{Name} - ");
        Console.WriteLine(e.Message);
    }

    public void Register(EventBus bus) => SubscribeInstanceHandler(bus);
}

// [EventHandlerContainer]
public partial class TestInheritedClass(string name, Action<string> handler) : TestBaseClass(name, handler)
{
    private readonly Action<string> _handler = handler;

    #pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
    [EventHandler]
    private async SyncTask AnotherTestEventHandler(AnotherTestEvent e)
    {
        _handler.Invoke(e.Message);
        Console.Write($"{Name} - ");
        Console.WriteLine(e.Message);
    }
    #pragma warning restore CS1998

    [EventHandler(["testA"])]
    private void TestEventHandlerInAnotherGroup(TestEvent e)
    {
        _handler.Invoke($"TestA:{e.Message}");
        Console.Write($"{Name} - ");
        Console.WriteLine($"TestA:{e.Message}");
    }

    public void RegisterTestA(EventBus bus) => SubscribeInstanceHandler(bus, "testA");
}

public record TestEvent(string Message) : BaseEvent;

public record AnotherTestEvent(string Message) : BaseEvent;