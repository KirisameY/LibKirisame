using KirisameY.BindingBridge.EventBinding.Binding;

namespace KirisameY.BindingBridge.EventBinding;

public static class EventEndpointExtensions
{
    extension<TSource, TDelegate>(IEventNotifierEndpoint<TSource, TDelegate> from) where TDelegate : Delegate where TSource : class
    {
        public IBindHandle EventBindTo<TTarget>(IEventHandlerEndpoint<TTarget, TDelegate> to, TSource source, TTarget target) =>
            new EventBinding<TSource, TTarget, TDelegate>(from, to, source, target);

        public IBindHandle EventBindTo<TTarget, TTargetDelegate>(
            IEventHandlerEndpoint<TTarget, TTargetDelegate> to,
            TSource source, TTarget target,
            Func<TTargetDelegate, TDelegate> converter
        ) where TTargetDelegate : Delegate =>
            new EventBinding<TSource, TTarget, TDelegate, TTargetDelegate>(from, to, source, target, converter);
    }
}