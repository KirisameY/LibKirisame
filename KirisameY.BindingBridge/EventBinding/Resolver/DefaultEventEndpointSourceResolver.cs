using KirisameY.BindingBridge.EventBinding.Implements;

namespace KirisameY.BindingBridge.EventBinding.Resolver;

public class DefaultEventEndpointSourceResolver : IEventEndpointSourceResolver
{
    private DefaultEventEndpointSourceResolver() { }
    public static DefaultEventEndpointSourceResolver Instance => new();

    public IEventNotifierEndpoint<TObject, TDelegate>? Resolve<TObject, TDelegate>(string eventName) where TObject : class where TDelegate : Delegate
    {
        var eventInfo = typeof(TObject).GetEvent(eventName);
        if (eventInfo is not { EventHandlerType: { } handlerType, AddMethod: { } addMethod, RemoveMethod: { } removeMethod }) return null;
        if (!typeof(TDelegate).IsAssignableTo(handlerType)) return null;

        var add = addMethod.CreateDelegate<Action<TObject, TDelegate>>();
        var remove = removeMethod.CreateDelegate<Action<TObject, TDelegate>>();
        return new DelegateEventNotifierEndpoint<TObject, TDelegate>(add, remove);
    }
}