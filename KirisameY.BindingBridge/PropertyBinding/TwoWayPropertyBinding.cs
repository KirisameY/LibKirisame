namespace KirisameY.BindingBridge.PropertyBinding;

internal sealed class TwoWayPropertyBinding<TSource, TTarget, TValue> : IBindHandle
{
    public TwoWayPropertyBinding(
        IUniversalPropertyEndpoint<TSource, TValue> from,
        IUniversalPropertyEndpoint<TTarget, TValue> to,
        TSource? source, TTarget? target
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

    private readonly IUniversalPropertyEndpoint<TSource, TValue> _from;
    private readonly IUniversalPropertyEndpoint<TTarget, TValue> _to;
    private readonly TSource? _source;
    private readonly TTarget? _target;

    private bool _disposed = false;

    private void Update(TValue value) => _to.SetValue(_target, value);
    private void UpdateReversed(TValue value) => _from.SetValue(_source, value);

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
        IUniversalPropertyEndpoint<TSource, TSourceValue> from,
        IUniversalPropertyEndpoint<TTarget, TTargetValue> to,
        TSource? source, TTarget? target,
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

    private readonly IUniversalPropertyEndpoint<TSource, TSourceValue> _from;
    private readonly IUniversalPropertyEndpoint<TTarget, TTargetValue> _to;
    private readonly TSource? _source;
    private readonly TTarget? _target;
    private readonly Func<TSourceValue, TTargetValue> _converter;
    private readonly Func<TTargetValue, TSourceValue> _reversedConverter;

    private bool _disposed = false;

    private void Update(TSourceValue value) => _to.SetValue(_target, _converter.Invoke(value));
    private void UpdateReversed(TTargetValue value) => _from.SetValue(_source, _reversedConverter.Invoke(value));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeUpdate(_source, Update);
        _to.UnsubscribeUpdate(_target, UpdateReversed);
    }
}