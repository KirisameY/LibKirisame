namespace KirisameY.BindingBridge.CollectionBinding.Binding;

internal sealed class ListBinding<TSource, TTarget, TItem> : IBindHandle
{
    public ListBinding(
        IListObservableEndpoint<TSource, TItem> from,
        IListObserverEndpoint<TTarget, TItem> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeListUpdate(_source, Added, Removed, Replaced, Moved, Reset);
        _to.ListReset(_target, _from.GetListView(_source));
    }

    private readonly IListObservableEndpoint<TSource, TItem> _from;
    private readonly IListObserverEndpoint<TTarget, TItem> _to;
    private readonly TSource _source;
    private readonly TTarget _target;

    private bool _disposed = false;

    private void Added(IEnumerable<TItem> added, IEnumerable<int>? indexes) => _to.ListItemAdded(_target, added, indexes);
    private void Removed(IEnumerable<TItem> removed, IEnumerable<int>? indexes) => _to.ListItemRemoved(_target, removed, indexes);
    private void Replaced(IEnumerable<TItem> old, IEnumerable<TItem> @new, IEnumerable<int>? indexes) => _to.ListItemReplaced(_target, old, @new, indexes);
    private void Moved(IEnumerable<TItem> items, IEnumerable<int> old, IEnumerable<int> @new) => _to.ListItemMoved(_target, items, old, @new);
    private void Reset(IReadOnlyList<TItem> view) => _to.ListReset(_target, view);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeListUpdate(_source, Added, Removed, Replaced, Moved, Reset);
    }
}