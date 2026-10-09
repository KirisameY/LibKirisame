namespace KirisameY.BindingBridge.EventBinding.Binding;

internal class EventBinding<TSource, TTarget, TDelegate> : IBindHandle where TSource : class where TDelegate : Delegate
{
    public EventBinding(
        IEventNotifierEndpoint<TSource, TDelegate> from,
        IEventHandlerEndpoint<TTarget, TDelegate> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _source = source;
        _to     = to.GetHandler(target);

        _from.SubscribeEvent(_source, _to);
    }

    private readonly IEventNotifierEndpoint<TSource, TDelegate> _from;
    private readonly TSource _source;
    private readonly TDelegate _to;

    private bool _disposed = false;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        _from.UnsubscribeEvent(_source, _to);
    }
}

internal class EventBinding<TSource, TTarget, TSourceDelegate, TTargetDelegate> : IBindHandle
    where TSource : class
    where TSourceDelegate : Delegate
    where TTargetDelegate : Delegate
{
    public EventBinding(
        IEventNotifierEndpoint<TSource, TSourceDelegate> from,
        IEventHandlerEndpoint<TTarget, TTargetDelegate> to,
        TSource source, TTarget target, Func<TTargetDelegate, TSourceDelegate> converter
    )
    {
        _from   = from;
        _source = source;
        _to     = converter.Invoke(to.GetHandler(target));

        _from.SubscribeEvent(_source, _to);
    }

    private readonly IEventNotifierEndpoint<TSource, TSourceDelegate> _from;
    private readonly TSourceDelegate _to;
    private readonly TSource _source;

    private bool _disposed = false;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        _from.UnsubscribeEvent(_source, _to);
    }
}