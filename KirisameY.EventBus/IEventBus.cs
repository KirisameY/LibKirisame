using KirisameY.SyncOrder;

namespace KirisameY.EventBus;

public interface IEventBus
{
    SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent;

    bool Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent;

    Order<TEvent> OrderPost<TEvent>(TEvent @event) where TEvent : BaseEvent;
}