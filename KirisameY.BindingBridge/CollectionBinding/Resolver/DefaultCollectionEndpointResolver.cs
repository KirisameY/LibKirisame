using System.Collections.Specialized;

using KirisameY.BindingBridge.CollectionBinding.Implements;

namespace KirisameY.BindingBridge.CollectionBinding.Resolver;

public class DefaultCollectionEndpointSourceResolver : ICollectionEndpointSourceResolver
{
    private DefaultCollectionEndpointSourceResolver() { }
    public static DefaultCollectionEndpointSourceResolver Instance => field ??= new();

    public ICollectionObservableEndpointBase<TObject, TElement>? Resolve<TObject, TElement>() where TObject : class
    {
        if (typeof(TObject).IsAssignableTo(typeof(IReadOnlyCollection<TElement>))) return null;
        if (typeof(TObject).IsAssignableTo(typeof(INotifyCollectionChanged))) return null;

        return typeof(TObject).IsAssignableTo(typeof(IReadOnlyList<TElement>))
            ? new NotifyListObservableEndpoint<TObject, TElement>(
                static o => (IReadOnlyList<TElement>)o,
                static o => (INotifyCollectionChanged)o
            )
            : new NotifyCollectionObservableEndpoint<TObject, TElement>(
                static o => (IReadOnlyCollection<TElement>)o,
                static o => (INotifyCollectionChanged)o
            );
    }
}

public class DefaultCollectionEndpointTargetResolver : ICollectionEndpointTargetResolver
{
    private DefaultCollectionEndpointTargetResolver() { }
    public static DefaultCollectionEndpointTargetResolver Instance => field ??= new();

    public ICollectionObserverEndpointBase<TObject, TElement>? Resolve<TObject, TElement>() where TObject : class
    {
        return typeof(TObject) switch
        {
            var t when t.IsAssignableTo(typeof(IList<TElement>)) =>
                new ModifiableListObservableEndpoint<TObject, TElement>(o => (IList<TElement>)o),
            var t when t.IsAssignableTo(typeof(ICollection<TElement>)) =>
                new ModifiableCollectionObservableEndpoint<TObject, TElement>(o => (ICollection<TElement>)o),
            _ => null
        };
    }
}