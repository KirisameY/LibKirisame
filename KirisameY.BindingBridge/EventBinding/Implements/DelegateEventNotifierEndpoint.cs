namespace KirisameY.BindingBridge.EventBinding.Implements;

internal class DelegateEventNotifierEndpoint<TObject, TDelegate>(
    Action<TObject, TDelegate> subscriber,
    Action<TObject, TDelegate> unsubscriber
) : IEventNotifierEndpoint<TObject, TDelegate>
    where TObject : class where TDelegate : Delegate
{
    public void SubscribeEvent(TObject obj, TDelegate handler)
    {
        subscriber.Invoke(obj, handler);
    }

    public void UnsubscribeEvent(TObject obj, TDelegate handler)
    {
        unsubscriber.Invoke(obj, handler);
    }
}