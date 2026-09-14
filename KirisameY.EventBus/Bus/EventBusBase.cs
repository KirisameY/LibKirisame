using System.Collections.Immutable;

using KirisameY.SyncOrder;

namespace KirisameY.EventBus.Bus;

public abstract class EventBusBase : IEventBus
{
    private readonly Dictionary<Type, HandlerInfos> _handlersDict = [];

    public SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent
    {
        if (!_handlersDict.TryGetValue(typeof(TEvent), out var infos))
        {
            infos = _handlersDict[typeof(TEvent)] = new();
        }

        infos.Handlers = infos.Handlers.Add((e =>
        {
            var @event = (TEvent)e;
            handler.Invoke(@event);
        }, handler));

        return new(() => Unsubscribe(handler));
    }

    public bool Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent
    {
        if (!_handlersDict.TryGetValue(typeof(TEvent), out var infos)) return false;

        (var prev, infos.Handlers) = (infos.Handlers, infos.Handlers.RemoveAll(t => t.source == (Delegate)handler));
        return prev.Count > infos.Handlers.Count;
    }


    private readonly Queue<Action> _eventPublishQueue = [];

    public Order<TEvent> OrderPost<TEvent>(TEvent @event) where TEvent : BaseEvent
    {
        OrderCompletionSource<TEvent> completionSource = new();
        return new(() =>
        {
            _eventPublishQueue.Enqueue(() =>
            {
                Type type = typeof(TEvent);
                while (type != typeof(object))
                {
                    if (_handlersDict.TryGetValue(type, out var infos))
                    {
                        infos.Handlers.ForEach(t => t.handler.Invoke(@event));
                    }
                    type = type.BaseType!;
                }
                completionSource.Complete(@event);
            });
            PostEnqueued();
        }, completionSource.Token);
    }

    protected abstract void PostEnqueued();

    protected void HandleQueue()
    {
        while (_eventPublishQueue.TryDequeue(out var publish))
        {
            publish.Invoke();
        }
    }


    private class HandlerInfos
    {
        public ImmutableList<(Action<BaseEvent> handler, Delegate? source)> Handlers { get; set; } = [];
    }
}