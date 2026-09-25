using KirisameY.BindingBridge.PropertyBinding.Binding;

namespace KirisameY.BindingBridge.PropertyBinding;

public static class PropertyEndpointExtensions
{
    extension<TSource, TTarget, TValue>(IObservablePropertyEndpoint<TSource, TValue> from)
    {
        public IBindHandle OneWayBindTo(IWritablePropertyEndpoint<TTarget, TValue> to, TSource? source, TTarget? target) =>
            new OneWayPropertyBinding<TSource, TTarget, TValue>(from, to, source, target);

        public IBindHandle OneWayBindTo<TTargetValue>(
            IWritablePropertyEndpoint<TTarget, TTargetValue> to,
            TSource? source, TTarget? target,
            Func<TValue, TTargetValue> converter
        ) => new OneWayPropertyBinding<TSource, TTarget, TValue, TTargetValue>(from, to, source, target, converter);
    }

    extension<TSource, TTarget, TValue>(IUniversalPropertyEndpoint<TSource, TValue> from)
    {
        public IBindHandle TwoWayBindTo(IUniversalPropertyEndpoint<TTarget, TValue> to, TSource? source, TTarget? target) =>
            new TwoWayPropertyBinding<TSource, TTarget, TValue>(from, to, source, target);

        public IBindHandle TwoWayBindTo<TTargetValue>(
            IUniversalPropertyEndpoint<TTarget, TTargetValue> to,
            TSource? source, TTarget? target,
            Func<TValue, TTargetValue> converter,
            Func<TTargetValue, TValue> reversedConverter
        ) => new TwoWayPropertyBinding<TSource, TTarget, TValue, TTargetValue>(from, to, source, target, converter, reversedConverter);
    }
}