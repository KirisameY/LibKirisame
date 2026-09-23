namespace KirisameY.BindingBridge.PropertyBinding;

internal sealed class OneWayPropertyBinding<TSource, TTarget, TValue> : IBindHandle
{
    public OneWayPropertyBinding(
        IObservablePropertyEndpoint<TSource, TValue> from,
        IWritablePropertyEndpoint<TTarget, TValue> to,
        TSource? source, TTarget? target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeUpdate(_source, Update);
        _to.SetValue(_target, _from.GetValue(_source));
    }

    private readonly IObservablePropertyEndpoint<TSource, TValue> _from;
    private readonly IWritablePropertyEndpoint<TTarget, TValue> _to;
    private readonly TSource? _source;
    private readonly TTarget? _target;

    private bool _disposed = false;

    private void Update(TValue value) => _to.SetValue(_target, value);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeUpdate(_source, Update);
    }
}

internal sealed class OneWayPropertyBinding<TSource, TTarget, TSourceValue, TTargetValue> : IBindHandle
{
    public OneWayPropertyBinding(
        IObservablePropertyEndpoint<TSource, TSourceValue> from,
        IWritablePropertyEndpoint<TTarget, TTargetValue> to,
        TSource? source, TTarget? target, Func<TSourceValue, TTargetValue> converter
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

    private readonly IObservablePropertyEndpoint<TSource, TSourceValue> _from;
    private readonly IWritablePropertyEndpoint<TTarget, TTargetValue> _to;
    private readonly TSource? _source;
    private readonly TTarget? _target;
    private readonly Func<TSourceValue, TTargetValue> _converter;

    private bool _disposed = false;

    private void Update(TSourceValue value) => _to.SetValue(_target, _converter.Invoke(value));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeUpdate(_source, Update);
    }
}