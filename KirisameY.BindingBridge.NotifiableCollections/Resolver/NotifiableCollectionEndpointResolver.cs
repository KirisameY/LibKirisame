using KirisameY.BindingBridge.CollectionBinding.Implements;
using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.BindingBridge.CollectionBinding.Resolver;

public class NotifiableCollectionSourceEndpointResolver(ICollectionEndpointSourceResolver? fallback) : ICollectionEndpointSourceResolver
{
    public ICollectionObservableEndpointBase<TObject, TElement>? Resolve<TObject, TElement>() where TObject : class
    {
        ICollectionObservableEndpointBase<TObject, TElement>? result = null;

        if (typeof(TObject).IsAssignableTo(typeof(IReadOnlyNotifiableList<TElement>)))
            result = new NotifiableListObservableEndpoint<TObject, TElement>(static o => (IReadOnlyNotifiableList<TElement>)o);
        else if (typeof(TObject).IsAssignableTo(typeof(IReadOnlyNotifiableCollection<TElement>)))
            result = new NotifiableCollectionObservableEndpoint<TObject, TElement>(static o => (IReadOnlyNotifiableCollection<TElement>)o);

        return result ?? fallback?.Resolve<TObject, TElement>();
    }
}