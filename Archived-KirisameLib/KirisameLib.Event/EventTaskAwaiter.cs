using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace KirisameLib.Event;

public readonly struct ConfiguredEventAwaitable<TEvent> where TEvent : BaseEvent
{
    internal ConfiguredEventAwaitable(EventTask<TEvent> task, bool contextRestore)
    {
        _task           = task;
        _contextRestore = contextRestore;
    }

    private readonly EventTask<TEvent> _task;
    private readonly bool _contextRestore;

    public EventAwaiter<TEvent> GetAwaiter() => new(_task, _contextRestore);
}

public readonly struct EventAwaiter<TEvent> : INotifyCompletion where TEvent : BaseEvent
{
    internal EventAwaiter(EventTask<TEvent> task, bool contextRestore = true)
    {
        _task           = task;
        _contextRestore = contextRestore;
    }

    private readonly EventTask<TEvent> _task;

    private readonly bool _contextRestore;

    public TEvent GetResult() => IsCompleted ? _task.Event : throw new InvalidOperationException("Event is not completed.");

    public bool IsCompleted => _task.IsCompleted;

    public void OnCompleted(Action continuation)
    {
        var context = _contextRestore ? SynchronizationContext.Current : null;

        _task.ContinueWith(() =>
        {
            if (context is not null)
            {
                if (context == SynchronizationContext.Current) continuation.Invoke();
                else context.Post(_ => continuation.Invoke(), null);
            }
            else continuation.Invoke();
        });

        _task.Ready();
    }
}