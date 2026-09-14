namespace KirisameY.EventBus.Test;

public record TestEvent(int Value) : BaseEvent;

public record OtherEvent(string Name) : BaseEvent;

public record DerivedTestEvent(int Value, string Tag) : TestEvent(Value);
