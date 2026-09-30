namespace KirisameY.BindingBridge.PropertyBinding.Binding;

internal sealed class OneWayPropertyBinding<TSource, TTarget, TValue> : IBindHandle
{
    public OneWayPropertyBinding(
        IPropertyObservableEndpoint<TSource, TValue> from,
        IPropertyWritableEndpoint<TTarget, TValue> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeUpdate(_source, Update);
        _to.SetValue(_target, _from.GetValue(_source));
    }

    private readonly IPropertyObservableEndpoint<TSource, TValue> _from;
    private readonly IPropertyWritableEndpoint<TTarget, TValue> _to;
    private readonly TSource _source;
    private readonly TTarget _target;

    private bool _disposed = false;

    private void Update() => _to.SetValue(_target, _from.GetValue(_source));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeUpdate(_source, Update);
    }
}

internal sealed class OneWayPropertyBinding<TSource, TTarget, TSourceValue, TTargetValue> : IBindHandle
{
    public OneWayPropertyBinding(
        IPropertyObservableEndpoint<TSource, TSourceValue> from,
        IPropertyWritableEndpoint<TTarget, TTargetValue> to,
        TSource source, TTarget target, Func<TSourceValue, TTargetValue> converter
    )
    {
        _from      = from;
        _to        = to;
        _source    = source;
        _target    = target;
        _converter = converter;

        _from.SubscribeUpdate(_source, Update);
        _to.SetValue(_target, _converter.Invoke(_from.GetValue(_source)));
    }

    private readonly IPropertyObservableEndpoint<TSource, TSourceValue> _from;
    private readonly IPropertyWritableEndpoint<TTarget, TTargetValue> _to;
    private readonly TSource _source;
    private readonly TTarget _target;
    private readonly Func<TSourceValue, TTargetValue> _converter;

    private bool _disposed = false;

    private void Update() => _to.SetValue(_target, _converter.Invoke(_from.GetValue(_source)));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeUpdate(_source, Update);
    }
}