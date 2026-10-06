using System.Collections.Frozen;

using KirisameY.BindingBridge.CollectionBinding.Resolver;
using KirisameY.BindingBridge.EventBinding.Resolver;
using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.Binder;

public readonly struct DataBinderBuilder()
{
    private readonly Dictionary<Type, IPropertyUpdateNotifyResolver> _propertyResolvers = [];
    private IPropertyUpdateNotifyResolver PropertyFallback { get; init; } = DefaultPropertyUpdateNotifyResolver.Instance;

    private readonly Dictionary<Type, ICollectionEndpointSourceResolver> _collectionSourceResolvers = [];
    private ICollectionEndpointSourceResolver CollectionSourceFallback { get; init; } = DefaultCollectionEndpointSourceResolver.Instance;

    private readonly Dictionary<Type, ICollectionEndpointTargetResolver> _collectionTargetResolvers = [];
    private ICollectionEndpointTargetResolver CollectionTargetFallback { get; init; } = DefaultCollectionEndpointTargetResolver.Instance;

    private readonly Dictionary<Type, IEventEndpointSourceResolver> _eventSourceResolvers = [];
    private IEventEndpointSourceResolver EventSourceFallback { get; init; } = DefaultEventEndpointSourceResolver.Instance;

    public DataBinderBuilder WithPropertyResolver(Type type, IPropertyUpdateNotifyResolver resolver)
    {
        _propertyResolvers.Add(type, resolver);
        return this;
    }

    public DataBinderBuilder WithPropertyFallbackResolver(IPropertyUpdateNotifyResolver fallback) => this with { PropertyFallback = fallback };

    public DataBinderBuilder WithCollectionSourceResolver(Type type, ICollectionEndpointSourceResolver resolver)
    {
        _collectionSourceResolvers.Add(type, resolver);
        return this;
    }

    public DataBinderBuilder WithCollectionSourceFallbackResolver(ICollectionEndpointSourceResolver fallback) => this with { CollectionSourceFallback = fallback };

    public DataBinderBuilder WithCollectionTargetResolver(Type type, ICollectionEndpointTargetResolver resolver)
    {
        _collectionTargetResolvers.Add(type, resolver);
        return this;
    }

    public DataBinderBuilder WithCollectionTargetFallbackResolver(ICollectionEndpointTargetResolver fallback) => this with { CollectionTargetFallback = fallback };

    public DataBinderBuilder WithEventSourceResolver(Type type, IEventEndpointSourceResolver resolver)
    {
        _eventSourceResolvers.Add(type, resolver);
        return this;
    }

    public DataBinderBuilder WithEventSourceFallbackResolver(IEventEndpointSourceResolver fallback) => this with { EventSourceFallback = fallback };

    public IDataBinder Build() => new RegistryDataBinder(
        _propertyResolvers.ToFrozenDictionary(), PropertyFallback,
        _collectionSourceResolvers.ToFrozenDictionary(), CollectionSourceFallback,
        _collectionTargetResolvers.ToFrozenDictionary(), CollectionTargetFallback,
        _eventSourceResolvers.ToFrozenDictionary(), EventSourceFallback
    );
}