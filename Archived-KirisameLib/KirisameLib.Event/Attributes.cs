#pragma warning disable CS9113

namespace KirisameLib.Event;

[AttributeUsage(AttributeTargets.Class)]
public sealed class EventHandlerContainerAttribute : Attribute;

/// <param name="groups">
///     Event handler groups.<br/>
///     <c>null</c>, <c>empty</c> or <c>string.Empty</c> for default group (with no prefix)
/// </param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class EventHandlerAttribute(string[]? groups = null, HandlerSubscribeFlag flag = HandlerSubscribeFlag.None) : Attribute;