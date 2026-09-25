using System.Linq.Expressions;

namespace KirisameY.BindingBridge.PropertyBinding.Resolver;

public readonly record struct PropertyUpdateNotifyProxy(Action<object?, Action> SubscribeUpdate, Action<object?, Action> UnsubscribeUpdate);