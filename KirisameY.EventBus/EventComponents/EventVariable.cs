using JetBrains.Annotations;

namespace KirisameY.EventBus.EventComponents;

[PublicAPI]
public class EventVariable<T>(T value)
{
    [PublicAPI]
    public T Value { get; set; } = value;
}