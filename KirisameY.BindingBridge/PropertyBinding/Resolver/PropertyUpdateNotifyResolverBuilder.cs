using System.Collections.Frozen;

namespace KirisameY.BindingBridge.PropertyBinding.Resolver;

public readonly struct PropertyUpdateNotifyResolverBuilder<TObject>()
{
    private readonly Dictionary<string, PropertyUpdateNotifyProxy> _notifies = [];
    private IPropertyUpdateNotifyResolver Fallback { get; init; } = DefaultPropertyUpdateNotifyResolver.Instance;

    public PropertyUpdateNotifyResolverBuilder<TObject> WithProperty(
        string name, Action<TObject, Action> subscribeUpdate, Action<TObject, Action> unsubscribeUpdate
    )
    {
        var notifyProxy = new PropertyUpdateNotifyProxy
        (
            (o, a) => subscribeUpdate((TObject)o, a), (o, a) => unsubscribeUpdate((TObject)o, a)
        );
        _notifies.Add(name, notifyProxy);
        return this;
    }

    public PropertyUpdateNotifyResolverBuilder<TObject> WithFallback(IPropertyUpdateNotifyResolver fallback) => this with { Fallback = fallback };

    public IPropertyUpdateNotifyResolver Build() => new RecordedPropertyUpdateNotifyResolver(_notifies.ToFrozenDictionary(), Fallback);
}