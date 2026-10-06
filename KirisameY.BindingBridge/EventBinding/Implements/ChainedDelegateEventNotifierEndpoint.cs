using System.Runtime.CompilerServices;

using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.EventBinding.Implements;

internal class ChainedDelegateEventNotifierEndpoint<TRoot, TNotifier, TDelegate>(
    IEventNotifierEndpoint<TNotifier, TDelegate> endpoint,
    Func<TRoot, TNotifier> notifierGetter,
    PropertyUpdateNotifyProxy? rebindNotify
) : IEventNotifierEndpoint<TRoot, TDelegate>
    where TRoot : class
    where TNotifier : class
    where TDelegate : Delegate
{
    private readonly ConditionalWeakTable<TRoot, Dictionary<TDelegate, Action>> _updateDicts = [];

    public void SubscribeEvent(TRoot obj, TDelegate handler)
    {
        var notifier = notifierGetter.Invoke(obj);
        endpoint.SubscribeEvent(notifier, handler);

        if (rebindNotify is null) return;

        var rebindHandler = () =>
        {
            var newNotifier = notifierGetter.Invoke(obj);
            endpoint.UnsubscribeEvent(notifier, handler);
            endpoint.SubscribeEvent(newNotifier, handler);
        };
        if (!_updateDicts.TryGetValue(obj, out var dict))
        {
            dict = [];
            _updateDicts.Add(obj, dict);
        }
        dict.Add(handler, rebindHandler);

        rebindNotify.Value.SubscribeUpdate(obj, rebindHandler);
    }

    public void UnsubscribeEvent(TRoot obj, TDelegate handler)
    {
        var notifier = notifierGetter.Invoke(obj);
        endpoint.UnsubscribeEvent(notifier, handler);

        if (rebindNotify is null) return;

        if (
            !_updateDicts.TryGetValue(obj, out var dict) ||
            !dict.Remove(handler, out var rebindHandler)
        ) throw new Exception("this exception should never be thrown");

        rebindNotify.Value.SubscribeUpdate(obj, rebindHandler);
    }
}