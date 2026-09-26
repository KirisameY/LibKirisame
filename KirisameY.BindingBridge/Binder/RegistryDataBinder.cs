using System.Collections.Frozen;
using System.Reflection;

using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.Binder;

internal class RegistryDataBinder(
    FrozenDictionary<Type, IPropertyUpdateNotifyResolver> propertyResolvers,
    IPropertyUpdateNotifyResolver propertyFallback
) : DataBinderBase
{
    protected override PropertyUpdateNotifyProxy? ResolveProperty(Type type, MemberInfo? memberInfo)
    {
        IPropertyUpdateNotifyResolver? resolver = null;

        var t = type;
        while (t is not null && !propertyResolvers.TryGetValue(t, out resolver))
        {
            t = t.BaseType;
        }
        resolver ??= propertyFallback;

        return resolver.Resolve(type, memberInfo);
    }
}