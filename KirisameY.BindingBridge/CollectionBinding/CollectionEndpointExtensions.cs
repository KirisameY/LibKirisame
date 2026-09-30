using KirisameY.BindingBridge.CollectionBinding.Binding;

namespace KirisameY.BindingBridge.CollectionBinding;

public static class CollectionEndpointExtensions
{
    extension<TSource, TTarget, TElement>(ICollectionObservableEndpoint<TSource, TElement> from)
    {
        public IBindHandle CollectionBindTo(
            ICollectionObserverEndpoint<TTarget, TElement> to,
            TSource source, TTarget target
        ) => new CollectionBinding<TSource, TTarget, TElement>(from, to, source, target);

        public IBindHandle CollectionBindToList(
            IListObserverEndpoint<TTarget, TElement> to,
            TSource source, TTarget target
        ) => new CollectionToListBinding<TSource, TTarget, TElement>(from, to, source, target);
    }

    extension<TSource, TTarget, TItem>(IListObservableEndpoint<TSource, TItem> from)
    {
        public IBindHandle ListBindTo(
            IListObserverEndpoint<TTarget, TItem> to,
            TSource source, TTarget target
        ) => new ListBinding<TSource, TTarget, TItem>(from, to, source, target);

        public IBindHandle ListBindToCollection(
            ICollectionObserverEndpoint<TTarget, TItem> to,
            TSource source, TTarget target
        ) => new ListToCollectionBinding<TSource, TTarget, TItem>(from, to, source, target);
    }
}