namespace KirisameY.BindingBridge.EventBinding.Resolver;

public interface IEventEndpointSourceResolver
{
    IEventNotifierEndpoint<TObject, TDelegate>? Resolve<TObject, TDelegate>(string eventName)
        where TObject : class where TDelegate : Delegate;
}