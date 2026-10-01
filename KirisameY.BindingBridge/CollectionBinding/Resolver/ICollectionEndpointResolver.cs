namespace KirisameY.BindingBridge.CollectionBinding.Resolver;

public interface ICollectionEndpointSourceResolver
{
    ICollectionObservableEndpointBase<TObject, TElement>? Resolve<TObject, TElement>() where TObject: class;
}

public interface ICollectionEndpointTargetResolver
{
    ICollectionObserverEndpointBase<TObject, TElement>? Resolve<TObject, TElement>() where TObject: class;
}