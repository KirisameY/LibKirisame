using System.Collections.Specialized;

using KirisameY.BindingBridge.CollectionBinding.Implements;
using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

namespace KirisameY.BindingBridge.CollectionBinding.Resolver;

public class DefaultCollectionEndpointSourceResolver : ICollectionEndpointSourceResolver
{
    private DefaultCollectionEndpointSourceResolver() { }
    public static DefaultCollectionEndpointSourceResolver Instance => field ??= new();

    public ICollectionObservableEndpointBase<TObject, TElement>? Resolve<TObject, TElement>() where TObject : class
    {
        if (!typeof(TObject).IsAssignableTo(typeof(IReadOnlyCollection<TElement>))) return null;
        if (!typeof(TObject).IsAssignableTo(typeof(INotifyCollectionChanged))) return null;

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
            var t when t.IsAssignableTo(typeof(ISimpleListObserver<TElement>)) =>
                new SimpleListObserverEndpoint<TObject, TElement>(o => (ISimpleListObserver<TElement>)o),
            var t when t.IsAssignableTo(typeof(ISimpleCollectionObserver<TElement>)) =>
                new SimpleCollectionObserverEndpoint<TObject, TElement>(o => (ISimpleCollectionObserver<TElement>)o),
            var t when t.IsAssignableTo(typeof(IList<TElement>)) =>
                new ModifiableListObserverEndpoint<TObject, TElement>(o => (IList<TElement>)o),
            var t when t.IsAssignableTo(typeof(ICollection<TElement>)) =>
                new ModifiableCollectionObserverEndpoint<TObject, TElement>(o => (ICollection<TElement>)o),
            _ => null
        };
    }
}