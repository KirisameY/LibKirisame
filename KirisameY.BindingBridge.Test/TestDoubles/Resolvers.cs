using System.Reflection;

using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     对任何输入都返回 <c>null</c> 的解析器：用来把"命中注册表"和"落到兜底"区分开。
/// </summary>
public sealed class NullPropertyUpdateNotifyResolver : IPropertyUpdateNotifyResolver
{
    /// <summary>按调用顺序记录下来每次解析请求。</summary>
    public List<(Type Type, string? MemberName)> Seen { get; } = [];

    public PropertyUpdateNotifyProxy? Resolve(Type type, MemberInfo? memberInfo)
    {
        Seen.Add((type, memberInfo?.Name));
        return null;
    }
}

/// <summary>
///     对任何输入都返回同一个空代理的解析器：用来验证"某个兜底被选中了"。
/// </summary>
public sealed class AlwaysResolvePropertyUpdateNotifyResolver : IPropertyUpdateNotifyResolver
{
    /// <summary>按调用顺序记录下来每次解析请求。</summary>
    public List<(Type Type, string? MemberName)> Seen { get; } = [];

    public PropertyUpdateNotifyProxy? Resolve(Type type, MemberInfo? memberInfo)
    {
        Seen.Add((type, memberInfo?.Name));
        return new PropertyUpdateNotifyProxy((_, _) => { }, (_, _) => { });
    }
}
