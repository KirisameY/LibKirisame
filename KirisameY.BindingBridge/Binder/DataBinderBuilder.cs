using System.Collections.Frozen;

using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.Binder;

public readonly struct DataBinderBuilder()
{
    private readonly Dictionary<Type, IPropertyUpdateNotifyResolver> _resolvers = [];
    private IPropertyUpdateNotifyResolver Fallback { get; init; } = DefaultPropertyUpdateNotifyResolver.Instance;

    public DataBinderBuilder WithResolver(Type type, IPropertyUpdateNotifyResolver resolver)
    {
        _resolvers.Add(type, resolver);
        return this;
    }

    public DataBinderBuilder WithFallbackResolver(IPropertyUpdateNotifyResolver fallback) => this with { Fallback = fallback };

    public IDataBinder Build() => new RegistryDataBinder(_resolvers.ToFrozenDictionary(), Fallback);
}