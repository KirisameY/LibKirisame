using System.Collections.Frozen;
using System.Reflection;

using KirisameY.BindingBridge.CollectionBinding;
using KirisameY.BindingBridge.CollectionBinding.Resolver;
using KirisameY.BindingBridge.EventBinding;
using KirisameY.BindingBridge.EventBinding.Resolver;
using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.Binder;

internal class RegistryDataBinder(
    FrozenDictionary<Type, IPropertyUpdateNotifyResolver> propertyResolvers,
    IPropertyUpdateNotifyResolver propertyFallback,
    FrozenDictionary<Type, ICollectionEndpointSourceResolver> collectionSourceResolvers,
    ICollectionEndpointSourceResolver collectionSourceFallback,
    FrozenDictionary<Type, ICollectionEndpointTargetResolver> collectionTargetResolvers,
    ICollectionEndpointTargetResolver collectionTargetFallback,
    FrozenDictionary<Type, IEventEndpointSourceResolver> eventSourceResolvers,
    IEventEndpointSourceResolver eventSourceFallback) : DataBinderBase
{
    protected override PropertyUpdateNotifyProxy? ResolveProperty(Type type, MemberInfo? memberInfo)
    {
        var resolver = TraceType(type)
                      .Select(propertyResolvers.GetValueOrDefault)
                      .FirstOrDefault(r => r is not null);

        return resolver?.Resolve(type, memberInfo) ?? propertyFallback.Resolve(type, memberInfo);
    }

    protected override ICollectionObservableEndpointBase<TObj, TElement>? ResolveCollectionSource<TObj, TElement>()
    {
        var resolver = TraceType(typeof(TObj))
                      .Select(collectionSourceResolvers.GetValueOrDefault)
                      .FirstOrDefault(r => r is not null);

        return resolver?.Resolve<TObj, TElement>() ?? collectionSourceFallback.Resolve<TObj, TElement>();
    }

    protected override ICollectionObserverEndpointBase<TObj, TElement>? ResolveCollectionTarget<TObj, TElement>()
    {
        var resolver = TraceType(typeof(TObj))
                      .Select(collectionTargetResolvers.GetValueOrDefault)
                      .FirstOrDefault(r => r is not null);

        return resolver?.Resolve<TObj, TElement>() ?? collectionTargetFallback.Resolve<TObj, TElement>();
    }

    protected override IEventNotifierEndpoint<TObject, TDelegate>? ResolveEventNotifierEndpoint<TObject, TDelegate>(string eventName)
    {
        var resolver = TraceType(typeof(TObject))
                      .Select(eventSourceResolvers.GetValueOrDefault)
                      .FirstOrDefault(r => r is not null);

        return resolver?.Resolve<TObject, TDelegate>(eventName) ?? eventSourceFallback.Resolve<TObject, TDelegate>(eventName);
    }

    private static IEnumerable<Type> TraceType(Type type)
    {
        var t = type;
        while (t is not null)
        {
            yield return t;
            t = t is { IsGenericType: true, IsGenericTypeDefinition: false }
                ? t.GetGenericTypeDefinition()
                : t.BaseType;
        }
    }
}