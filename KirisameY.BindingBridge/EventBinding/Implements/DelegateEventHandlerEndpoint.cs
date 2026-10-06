namespace KirisameY.BindingBridge.EventBinding.Implements;

internal class DelegateEventHandlerEndpoint<TObject, TDelegate>(
    Func<TObject, TDelegate> handlerGetter
) : IEventHandlerEndpoint<TObject, TDelegate> where TDelegate : Delegate
{
    public TDelegate GetHandler(TObject obj) => handlerGetter.Invoke(obj);
}