using System.Collections.ObjectModel;

using KirisameY.BindingBridge.CollectionBinding;
using KirisameY.BindingBridge.CollectionBinding.Resolver;
using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.BindingBridge.NotifiableCollections.Test.ResolverTests;

/// <summary>
///     <c>NotifiableCollectionSourceEndpointResolver</c>：只认 Notifiable 那两族只读接口、列表优先，
///     认不出来就交给构造时传进来的下层解析器。
/// </summary>
public class NotifiableCollectionSourceEndpointResolverTests
{
    /// <summary>什么类型都不认、只记下"被问过哪些类型"的下层解析器。</summary>
    private sealed class RecordingSourceResolver : ICollectionEndpointSourceResolver
    {
        public List<Type> Seen { get; } = [];

        public ICollectionObservableEndpointBase<TObject, TElement>? Resolve<TObject, TElement>() where TObject : class
        {
            Seen.Add(typeof(TObject));
            return null;
        }
    }

    [Fact]
    public void ANotifiableListIsResolvedAsAListEndpoint()
    {
        var resolver = new NotifiableCollectionSourceEndpointResolver(null);

        var endpoint = resolver.Resolve<NotifiableList<int>, int>();

        Assert.IsAssignableFrom<IListObservableEndpoint<NotifiableList<int>, int>>(endpoint);
    }

    [Fact]
    public void ANonListNotifiableCollectionIsResolvedAsACollectionEndpoint()
    {
        var resolver = new NotifiableCollectionSourceEndpointResolver(null);

        var endpoint = resolver.Resolve<IReadOnlyNotifiableCollection<int>, int>();

        Assert.IsAssignableFrom<ICollectionObservableEndpoint<IReadOnlyNotifiableCollection<int>, int>>(endpoint);
        Assert.IsNotAssignableFrom<IListObservableEndpoint<IReadOnlyNotifiableCollection<int>, int>>(endpoint);
    }

    [Fact]
    public void ANotifiableTypeIsHandledWithoutAskingTheFallback()
    {
        var fallback = new RecordingSourceResolver();
        var resolver = new NotifiableCollectionSourceEndpointResolver(fallback);

        var endpoint = resolver.Resolve<NotifiableList<int>, int>();

        Assert.IsAssignableFrom<IListObservableEndpoint<NotifiableList<int>, int>>(endpoint);
        Assert.DoesNotContain(typeof(NotifiableList<int>), fallback.Seen);
    }

    [Fact]
    public void AnUnrecognizedTypeIsLeftToTheFallback()
    {
        var resolver = new NotifiableCollectionSourceEndpointResolver(DefaultCollectionEndpointSourceResolver.Instance);

        var endpoint = resolver.Resolve<ObservableCollection<int>, int>();

        Assert.IsAssignableFrom<IListObservableEndpoint<ObservableCollection<int>, int>>(endpoint);
    }

    [Fact]
    public void WithoutAFallbackAnUnrecognizedTypeResolvesToNothing()
    {
        var resolver = new NotifiableCollectionSourceEndpointResolver(null);

        Assert.Null(resolver.Resolve<ObservableCollection<int>, int>());
    }
}
