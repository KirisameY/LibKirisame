namespace KirisameY.BindingBridge.CollectionBinding.Binding;

internal class ListToCollectionBinding<TSource, TTarget, TItem> : IBindHandle
{
    public ListToCollectionBinding(
        IListObservableEndpoint<TSource, TItem> from,
        ICollectionObserverEndpoint<TTarget, TItem> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeListUpdate(_source, Added, Removed, Replaced, Moved, Reset);
        _to.CollectionReset(_target, _from.GetListView(_source));
    }

    private readonly IListObservableEndpoint<TSource, TItem> _from;
    private readonly ICollectionObserverEndpoint<TTarget, TItem> _to;
    private readonly TSource _source;
    private readonly TTarget _target;

    private bool _disposed = false;

    private void Added(IEnumerable<TItem> added, IEnumerable<int>? indexes) => _to.CollectionItemAdded(_target, added);
    private void Removed(IEnumerable<TItem> removed, IEnumerable<int>? indexes) => _to.CollectionItemRemoved(_target, removed);
    private void Replaced(IEnumerable<TItem> old, IEnumerable<TItem> @new, IEnumerable<int>? indexes) => _to.CollectionItemReplaced(_target, old, @new);
    private void Moved(IEnumerable<TItem> items, IEnumerable<int> old, IEnumerable<int> @new) { }
    private void Reset(IReadOnlyList<TItem> view) => _to.CollectionReset(_target, view);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeListUpdate(_source, Added, Removed, Replaced, Moved, Reset);
    }
}