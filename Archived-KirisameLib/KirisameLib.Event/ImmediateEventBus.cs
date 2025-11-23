namespace KirisameLib.Event;

public sealed class ImmediateEventBus(Action<BaseEvent, Exception> exceptionHandler) : EventBus(exceptionHandler)
{
    private bool _handlingEvent = false;

    protected override void EventReceived()
    {
        if (_handlingEvent) return;

        //handle event queue
        _handlingEvent = true;
        List<EventSendingException> exceptions = [];

        while (NotifyQueue.TryDequeue(out var notifyAction))
        {
            try
            {
                notifyAction.Invoke();
            }
            catch (EventSendingException e)
            {
                exceptions.Add(e);
            }
        }
        _handlingEvent = false;

        if (exceptions.Count > 0) throw new QueueEventSendingException(exceptions);
    }
}