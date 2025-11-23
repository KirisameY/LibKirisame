using System.Collections.Immutable;

using KirisameLib.Asynchronous.SyncTasking;
using KirisameLib.Extensions;

namespace KirisameLib.Event;

public abstract class EventBus(Action<BaseEvent, Exception> exceptionHandler)
{
    private readonly Dictionary<Type, HandlerInfos> _handlersDict = new();
    protected readonly Queue<Action> NotifyQueue = [];


    /// <returns>
    ///     Callback to remove registered handler, useful when unregistering an anonymous function
    /// </returns>
    #pragma warning disable CS1998 // async method without await
    public Action Subscribe<TEvent>(Action<TEvent> handler, HandlerSubscribeFlag flags = HandlerSubscribeFlag.None)
        where TEvent : BaseEvent =>
        SubscribeHandler(async e => handler(e), handler, flags);
    #pragma warning restore CS1998

    /// <returns>
    ///     Callback to remove registered handler, useful when unregistering an anonymous function
    /// </returns>
    public Action SubscribeAsync<TEvent>(Func<TEvent, SyncTask> handler, HandlerSubscribeFlag flags = HandlerSubscribeFlag.None)
        where TEvent : BaseEvent =>
        SubscribeHandler(handler, null, flags);

    private Action SubscribeHandler<TEvent>(Func<TEvent, SyncTask> handler, Action<TEvent>? source, HandlerSubscribeFlag flags) where TEvent : BaseEvent
    {
        if (!_handlersDict.TryGetValue(typeof(TEvent), out var handlerInfos))
            _handlersDict[typeof(TEvent)] = handlerInfos = new();

        handlerInfos.Handlers = handlerInfos.Handlers.Add((handler, source));
        //record only once handlers
        if (flags.HasFlag(HandlerSubscribeFlag.OnlyOnce)) handlerInfos.OneTimeHandlers.Add(handler);
        return () => Unsubscribe(handler);
    }


    public void Unsubscribe<TEvent>(Action<TEvent> handler)
        where TEvent : BaseEvent
    {
        if (!_handlersDict.TryGetValue(typeof(TEvent), out var handlerInfos))
            _handlersDict[typeof(TEvent)] = handlerInfos = new();

        handlerInfos.Handlers = handlerInfos.Handlers.RemoveAll(h => ReferenceEquals(h.source, handler));
    }

    public void Unsubscribe<TEvent>(Func<TEvent, SyncTask> handler)
        where TEvent : BaseEvent
    {
        if (!_handlersDict.TryGetValue(typeof(TEvent), out var handlerInfos))
            _handlersDict[typeof(TEvent)] = handlerInfos = new();

        handlerInfos.Handlers = handlerInfos.Handlers.RemoveAll(h => ReferenceEquals(h.handler, handler));
    }


    private void EmitEvent<TEvent>(TEvent @event, Action? continueAction = null) where TEvent : BaseEvent
    {
        List<Exception> exceptions = [];
        List<SyncTask> tasks = [];

        // 遍历Event类型基类直至BaseEvent(其基类是object)
        for (var type = typeof(TEvent); type != typeof(object); type = type!.BaseType)
        {
            if (!_handlersDict.TryGetValue(type!, out var handlerInfos)) continue;

            // 记录并清空当前一次性handlers
            var oneTimeHandlers = handlerInfos.OneTimeHandlers.ToArray();
            handlerInfos.OneTimeHandlers.Clear();

            // 监听者产生的异常集中处理
            foreach (var handler in handlerInfos.Handlers.Select(h => h.handler))
            {
                try
                {
                    var handlerDelegate = (Func<TEvent, SyncTask>)handler;
                    var result = handlerDelegate.Invoke(@event);
                    result.ContinueWith(t =>
                    {
                        if (!t.IsFaulted) return;
                        exceptionHandler.Invoke(@event, t.Exception!.Flatten());
                    });
                    tasks.Add(result);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            // 移除一次性Handlers
            oneTimeHandlers.ForEach(h =>
            {
                handlerInfos.Handlers = handlerInfos.Handlers.RemoveAll(h1 => ReferenceEquals(h1.handler, h));
            });
        }

        SyncTask.WhenAll(tasks).ContinueWith(_ =>
        {
            try { continueAction?.Invoke(); }
            catch (Exception e) { exceptionHandler.Invoke(@event, e); }
        });

        if (exceptions.Count > 0) throw new EventSendingException(exceptions, @event);
    }

    protected abstract void EventReceived();

    protected virtual void NotifyEnqueue(Action action)
    {
        NotifyQueue.Enqueue(action);
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : BaseEvent
    {
        NotifyEnqueue(() => EmitEvent(@event));
        EventReceived();
    }

    public EventTask<TEvent> PublishAndWaitFor<TEvent>(TEvent @event) where TEvent : BaseEvent
    {
        return new(@event, task =>
        {
            NotifyEnqueue(() => EmitEvent(task.Event, task.Complete));
            EventReceived();
        });
    }


    private class HandlerInfos
    {
        public ImmutableList<(Delegate handler, Delegate? source)> Handlers { get; set; } = [];
        public List<Delegate> OneTimeHandlers { get; } = [];
    }
}