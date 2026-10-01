namespace KirisameY.BindingBridge.CollectionBinding.Binding;

internal sealed class CollectionBinding<TSource, TTarget, TElement> : IBindHandle where TSource : class where TTarget : class
{
    public CollectionBinding(
        ICollectionObservableEndpoint<TSource, TElement> from,
        ICollectionObserverEndpoint<TTarget, TElement> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeCollectionUpdate(_source, Added, Removed, Replaced, Reset);
        _to.CollectionReset(_target, _from.GetCollectionView(_source));
    }

    private readonly ICollectionObservableEndpoint<TSource, TElement> _from;
    private readonly ICollectionObserverEndpoint<TTarget, TElement> _to;
    private readonly TSource _source;
    private readonly TTarget _target;

    private bool _disposed = false;

    private void Added(IEnumerable<TElement> added) => _to.CollectionItemAdded(_target, added);
    private void Removed(IEnumerable<TElement> removed) => _to.CollectionItemRemoved(_target, removed);
    private void Replaced(IEnumerable<TElement> old, IEnumerable<TElement> @new) => _to.CollectionItemReplaced(_target, old, @new);
    private void Reset(IReadOnlyCollection<TElement> view) => _to.CollectionReset(_target, view);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeCollectionUpdate(_source, Added, Removed, Replaced, Reset);
    }
}