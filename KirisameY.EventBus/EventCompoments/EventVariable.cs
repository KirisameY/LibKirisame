using JetBrains.Annotations;

namespace KirisameY.EventBus.EventCompoments;

[PublicAPI]
public struct EventVariable<T>(T value)
{
    [PublicAPI]
    public T Value { get; set; } = value;
}