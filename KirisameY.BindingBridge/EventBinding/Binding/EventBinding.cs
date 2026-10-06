namespace KirisameY.BindingBridge.EventBinding.Binding;

internal class EventBinding<TSource, TTarget, TDelegate> : IBindHandle where TDelegate : Delegate where TSource : class
{
    public EventBinding(
        IEventNotifierEndpoint<TSource, TDelegate> from,
        IEventHandlerEndpoint<TTarget, TDelegate> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeEvent(_source, _to.GetHandler(_target));
    }

    private readonly IEventNotifierEndpoint<TSource, TDelegate> _from;
    private readonly IEventHandlerEndpoint<TTarget, TDelegate> _to;
    private readonly TSource _source;
    private readonly TTarget _target;

    private bool _disposed = false;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        _from.UnsubscribeEvent(_source, _to.GetHandler(_target));
    }
}