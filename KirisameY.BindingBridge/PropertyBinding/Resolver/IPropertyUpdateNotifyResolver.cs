using System.Reflection;

namespace KirisameY.BindingBridge.PropertyBinding.Resolver;

public interface IPropertyUpdateNotifyResolver
{
    // memberInfo should be Property, Field or Indexer (indexer info maybe null when index for an array)
    PropertyUpdateNotifyProxy? Resolve(Type type, MemberInfo? memberInfo);
}