using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace KirisameLib.Event;

public class EventSendingException(IEnumerable<Exception> innerExceptions, BaseEvent @event) : AggregateException(innerExceptions)
{
    public BaseEvent FromEvent => @event;
    public override string ToString() => $"FromEvent:{FromEvent}, {base.ToString()}";
}

public class QueueEventSendingException(IEnumerable<EventSendingException> innerExceptions) : AggregateException(innerExceptions)
{
    [field: AllowNull, MaybeNull]
    public ReadOnlyCollection<EventSendingException> EventHandlingExceptions => field ??= new(innerExceptions.ToList());
}