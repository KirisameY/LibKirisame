using KirisameY.BindingBridge.PropertyBinding.Binding;

namespace KirisameY.BindingBridge.PropertyBinding;

public static class PropertyEndpointExtensions
{
    extension<TSource, TTarget, TValue>(IPropertyObservableEndpoint<TSource, TValue> from)
    {
        public IBindHandle OneWayBindTo(IPropertyWritableEndpoint<TTarget, TValue> to, TSource source, TTarget target) =>
            new OneWayPropertyBinding<TSource, TTarget, TValue>(from, to, source, target);

        public IBindHandle OneWayBindTo<TTargetValue>(
            IPropertyWritableEndpoint<TTarget, TTargetValue> to,
            TSource source, TTarget target,
            Func<TValue, TTargetValue> converter
        ) => new OneWayPropertyBinding<TSource, TTarget, TValue, TTargetValue>(from, to, source, target, converter);
    }

    extension<TSource, TTarget, TValue>(IPropertyUniversalEndpoint<TSource, TValue> from)
    {
        public IBindHandle TwoWayBindTo(IPropertyUniversalEndpoint<TTarget, TValue> to, TSource source, TTarget target) =>
            new TwoWayPropertyBinding<TSource, TTarget, TValue>(from, to, source, target);

        public IBindHandle TwoWayBindTo<TTargetValue>(
            IPropertyUniversalEndpoint<TTarget, TTargetValue> to,
            TSource source, TTarget target,
            Func<TValue, TTargetValue> converter,
            Func<TTargetValue, TValue> reversedConverter
        ) => new TwoWayPropertyBinding<TSource, TTarget, TValue, TTargetValue>(from, to, source, target, converter, reversedConverter);
    }
}