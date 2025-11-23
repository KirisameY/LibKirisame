namespace KirisameLib.Event;

public class EventTask<TEvent> where TEvent : BaseEvent
{
    internal EventTask(TEvent @event, Action<EventTask<TEvent>> readyAction)
    {
        Event        = @event;
        _readyAction = readyAction;
    }

    private readonly Action<EventTask<TEvent>> _readyAction;
    private Action? _continueAction;

    private readonly Lock _lock = new();
    public bool IsReady { get; private set; } = false;
    public bool IsCompleted { get; private set; } = false;

    public TEvent Event { get; private init; }


    internal void Complete()
    {
        Action? actionToInvoke;

        lock (_lock)
        {
            if (IsCompleted) return;
            IsCompleted    = true;
            actionToInvoke = _continueAction;
        }

        actionToInvoke?.Invoke();
    }

    public EventAwaiter<TEvent> GetAwaiter() => new(this);

    public EventTask<TEvent> ContinueWith(Action continuation)
    {
        bool completed = false;
        lock (_lock)
        {
            if (IsCompleted) completed =  true;
            else _continueAction       += continuation;
        }

        if (completed) continuation.Invoke();
        return this;
    }

    public EventTask<TEvent> ContinueWith(Action<TEvent> continuation)
    {
        var action = () => continuation.Invoke(Event);

        bool completed = false;
        lock (_lock)
        {
            if (IsCompleted) completed =  true;
            else _continueAction       += action;
        }

        if (completed) action.Invoke();
        return this;
    }

    public void Ready()
    {
        if (IsReady) return;
        IsReady = true;
        _readyAction.Invoke(this);
    }

    public ConfiguredEventAwaitable<TEvent> ConfigureAwait(bool continueOnCapturedContext) => new(this, continueOnCapturedContext);
}