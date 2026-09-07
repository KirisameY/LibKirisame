namespace KirisameY.EventBus;

public readonly struct SubscriptionToken(Action unsubscribe) : IDisposable
{
    public void Dispose() => unsubscribe.Invoke();
}