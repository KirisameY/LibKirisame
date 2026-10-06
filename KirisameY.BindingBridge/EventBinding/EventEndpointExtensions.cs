using KirisameY.BindingBridge.EventBinding.Binding;

namespace KirisameY.BindingBridge.EventBinding;

public static class EventEndpointExtensions
{
    extension<TSource, TDelegate>(IEventNotifierEndpoint<TSource, TDelegate> from) where TDelegate : Delegate where TSource : class
    {
        public IBindHandle EventBindTo<TTarget>(IEventHandlerEndpoint<TTarget, TDelegate> to, TSource source, TTarget target) =>
            new EventBinding<TSource, TTarget, TDelegate>(from, to, source, target);
    }
}