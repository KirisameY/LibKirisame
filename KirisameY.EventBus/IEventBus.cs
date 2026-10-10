namespace KirisameY.EventBus;

public interface IEventBus<TBaseEvent> where TBaseEvent : BaseEvent
{
    SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : TBaseEvent;

    bool Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : TBaseEvent;

    void Publish<TEvent>(TEvent @event) where TEvent : TBaseEvent;
}