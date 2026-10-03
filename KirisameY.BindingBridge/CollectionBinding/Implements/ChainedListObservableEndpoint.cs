using System.Runtime.CompilerServices;

using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.CollectionBinding.Implements;

internal class ChainedListObservableEndpoint<TObj, TList, TItem>(
    IListObservableEndpoint<TList, TItem> endpoint,
    Func<TObj, TList> listGetter,
    PropertyUpdateNotifyProxy? notify
) : IListObservableEndpoint<TObj, TItem>
    where TObj : class where TList : class
{
    public IReadOnlyList<TItem> GetListView(TObj obj) => endpoint.GetListView(listGetter.Invoke(obj));

    private readonly ConditionalWeakTable<
        TObj, Dictionary<(
            ListItemAddedHandler<TItem> addHandler,
            ListItemRemovedHandler<TItem> removedHandler,
            ListItemReplacedHandler<TItem> replacedHandler,
            ListItemMovedHandler<TItem> movedHandler,
            ListResetHandler<TItem> resetHandler
            ), Action>
    > _updateDicts = [];

    public void SubscribeListUpdate(
        TObj obj,
        ListItemAddedHandler<TItem> addHandler,
        ListItemRemovedHandler<TItem> removedHandler,
        ListItemReplacedHandler<TItem> replacedHandler,
        ListItemMovedHandler<TItem> movedHandler,
        ListResetHandler<TItem> resetHandler
    )
    {
        var list = listGetter.Invoke(obj);
        endpoint.SubscribeListUpdate(list, addHandler, removedHandler, replacedHandler, movedHandler, resetHandler);

        if (notify is null) return;

        var handler = () =>
        {
            var newList = listGetter.Invoke(obj);
            endpoint.UnsubscribeListUpdate(list, addHandler, removedHandler, replacedHandler, movedHandler, resetHandler);
            endpoint.SubscribeListUpdate(newList, addHandler, removedHandler, replacedHandler, movedHandler, resetHandler);
            resetHandler.Invoke(endpoint.GetListView(newList));
        };
        if (!_updateDicts.TryGetValue(obj, out var dict))
        {
            dict = [];
            _updateDicts.Add(obj, dict);
        }
        dict.Add((addHandler, removedHandler, replacedHandler, movedHandler, resetHandler), handler);

        notify.Value.SubscribeUpdate(obj, handler);
    }

    public void UnsubscribeListUpdate(
        TObj obj,
        ListItemAddedHandler<TItem> addHandler,
        ListItemRemovedHandler<TItem> removedHandler,
        ListItemReplacedHandler<TItem> replacedHandler,
        ListItemMovedHandler<TItem> movedHandler,
        ListResetHandler<TItem> resetHandler
    )
    {
        var list = listGetter.Invoke(obj);
        endpoint.UnsubscribeListUpdate(list, addHandler, removedHandler, replacedHandler, movedHandler, resetHandler);

        if (notify is null) return;

        if (
            !_updateDicts.TryGetValue(obj, out var dict) ||
            !dict.Remove((addHandler, removedHandler, replacedHandler, movedHandler, resetHandler), out var handler)
        ) throw new Exception("this exception should not be thrown");

        notify.Value.UnsubscribeUpdate(obj, handler);
    }
}