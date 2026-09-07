namespace KirisameY.EventBus;

public interface IEventBus
{
    SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    void Post<TEvent>(TEvent @event) where TEvent : class;
}