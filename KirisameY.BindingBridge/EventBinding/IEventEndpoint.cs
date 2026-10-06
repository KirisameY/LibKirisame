namespace KirisameY.BindingBridge.EventBinding;

public interface IEventNotifierEndpoint<in TObject, in TDelegate>
    where TObject : class
    where TDelegate : Delegate
{
    void SubscribeEvent(TObject obj, TDelegate handler);
    void UnsubscribeEvent(TObject obj, TDelegate handler);
}

public interface IEventHandlerEndpoint<in TObject, out TDelegate>
    where TDelegate : Delegate
{
    TDelegate GetHandler(TObject obj);
}