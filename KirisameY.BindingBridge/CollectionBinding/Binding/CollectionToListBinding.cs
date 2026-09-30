using System.Collections.Immutable;

namespace KirisameY.BindingBridge.CollectionBinding.Binding;

internal sealed class CollectionToListBinding<TSource, TTarget, TElement> : IBindHandle
{
    public CollectionToListBinding(
        ICollectionObservableEndpoint<TSource, TElement> from,
        IListObserverEndpoint<TTarget, TElement> to,
        TSource source, TTarget target
    )
    {
        _from   = from;
        _to     = to;
        _source = source;
        _target = target;

        _from.SubscribeCollectionUpdate(_source, Added, Removed, Replaced, Reset);
        _to.ListReset(_target, [.._from.GetCollectionView(_source)]);
    }

    private readonly ICollectionObservableEndpoint<TSource, TElement> _from;
    private readonly IListObserverEndpoint<TTarget, TElement> _to;
    private readonly TSource _source;
    private readonly TTarget _target;

    private bool _disposed = false;

    private void Added(IEnumerable<TElement> added) => _to.ListItemAdded(_target, added, null);
    private void Removed(IEnumerable<TElement> removed) => _to.ListItemRemoved(_target, removed, null);
    private void Replaced(IEnumerable<TElement> old, IEnumerable<TElement> @new) => _to.ListItemReplaced(_target, old, @new, null);
    private void Reset(IReadOnlyCollection<TElement> view) => _to.ListReset(_target, [..view]);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        _from.UnsubscribeCollectionUpdate(_source, Added, Removed, Replaced, Reset);
    }
}