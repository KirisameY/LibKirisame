using System.Collections.Immutable;

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

    public void Publish<TEvent>(TEvent @event) where TEvent : BaseEvent
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
    }


    private class HandlerInfos
    {
        public ImmutableList<(Action<BaseEvent> handler, Delegate? source)> Handlers { get; set; } = [];
    }
}