namespace KirisameY.BindingBridge.PropertyBinding.Binding;

internal sealed class TwoWayPropertyBinding<TSource, TTarget, TValue> : IBindHandle
{
    public TwoWayPropertyBinding(
        IPropertyUniversalEndpoint<TSource, TValue> from,
        IPropertyUniversalEndpoint<TTarget, TValue> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeUpdate(_source, Update);
        _to.SubscribeUpdate(_target, UpdateReversed);

        _to.SetValue(_target, _from.GetValue(_source));
    }

    private readonly IPropertyUniversalEndpoint<TSource, TValue> _from;
    private readonly IPropertyUniversalEndpoint<TTarget, TValue> _to;
    private readonly TSource _source;
    private readonly TTarget _target;

    private bool _disposed = false;

    private void Update() => _to.SetValue(_target, _from.GetValue(_source));
    private void UpdateReversed() => _from.SetValue(_source, _to.GetValue(_target));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeUpdate(_source, Update);
        _to.UnsubscribeUpdate(_target, UpdateReversed);
    }
}

internal sealed class TwoWayPropertyBinding<TSource, TTarget, TSourceValue, TTargetValue> : IBindHandle
{
    public TwoWayPropertyBinding(
        IPropertyUniversalEndpoint<TSource, TSourceValue> from,
        IPropertyUniversalEndpoint<TTarget, TTargetValue> to,
        TSource source, TTarget target,
        Func<TSourceValue, TTargetValue> converter,
        Func<TTargetValue, TSourceValue> reversedConverter
    )
    {
        _from              = from;
        _to                = to;
        _source            = source;
        _target            = target;
        _converter         = converter;
        _reversedConverter = reversedConverter;

        _from.SubscribeUpdate(_source, Update);
        _to.SubscribeUpdate(_target, UpdateReversed);

        _to.SetValue(_target, _converter.Invoke(_from.GetValue(_source)));
    }

    private readonly IPropertyUniversalEndpoint<TSource, TSourceValue> _from;
    private readonly IPropertyUniversalEndpoint<TTarget, TTargetValue> _to;
    private readonly TSource _source;
    private readonly TTarget _target;
    private readonly Func<TSourceValue, TTargetValue> _converter;
    private readonly Func<TTargetValue, TSourceValue> _reversedConverter;

    private bool _disposed = false;

    private void Update() => _to.SetValue(_target, _converter.Invoke(_from.GetValue(_source)));
    private void UpdateReversed() => _from.SetValue(_source, _reversedConverter.Invoke(_to.GetValue(_target)));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeUpdate(_source, Update);
        _to.UnsubscribeUpdate(_target, UpdateReversed);
    }
}