namespace KirisameY.EventBus;

public interface IEventBus
{
    SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent;

    bool Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent;

    void Publish<TEvent>(TEvent @event) where TEvent : BaseEvent;
}