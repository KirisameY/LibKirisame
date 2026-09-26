using System.Collections.Frozen;
using System.Reflection;

namespace KirisameY.BindingBridge.PropertyBinding.Resolver;

internal class RecordedPropertyUpdateNotifyResolver(
    FrozenDictionary<string, PropertyUpdateNotifyProxy> notifies,
    IPropertyUpdateNotifyResolver fallback
) : IPropertyUpdateNotifyResolver
{
    public PropertyUpdateNotifyProxy? Resolve(Type type, MemberInfo? memberInfo)
    {
        if (memberInfo is not null && notifies.TryGetValue(memberInfo.Name, out var value)) return value;
        return fallback.Resolve(type, memberInfo);
    }
}