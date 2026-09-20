using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

/// <param name="notifyThreshold">
///     决定集合更新时应分多次发出单项通知还是一次性发出 Reset 通知<br/>
///     > <b>正值</b>：在发生变化的集合项数量大于该值时将发送单次 Reset
///     > <b>0</b>：始终发出单次通知
///     > <b>负值</b>：始终发出单次 Reset
/// </param>
internal class ObservableCollectionWrapper<T>(IReadOnlyNotifiableCollection<T> source, int notifyThreshold = 0) : IReadOnlyObservableCollection<T>
{
    public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Count;


    // Events

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => source.CollectionUpdated += value is null ? null : CollectionChangedHandlerCache.WrapNew(value);
        remove => source.CollectionUpdated -= value is null ? null : CollectionChangedHandlerCache.RemoveWrapped(value);
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => source.CollectionUpdated += value is null ? null : PropertyChangedHandlerCache.WrapNew(value);
        remove => source.CollectionUpdated -= value is null ? null : PropertyChangedHandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        NotifyCollectionChangedEventHandler,
        EventHandler<CollectionUpdateEventArgs<T>>
    > CollectionChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        IEnumerable<NotifyCollectionChangedEventArgs> newArgs = args switch
        {
            ICollectionItemAddedEventArgs<T> added => (notifyThreshold, added.AddedItems) switch
            {
                (>= 0 and var t, var items) when t == 0 || items.Count < t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            ICollectionItemRemovedEventArgs<T> removed => (notifyThreshold, removed.RemovedItems) switch
            {
                (>= 0 and var t, var items) when t == 0 || items.Count < t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            ICollectionItemReplacedEventArgs<T> replaced => (notifyThreshold, replaced.ItemChanges) switch
            {
                (>= 0 and var t, var changes) when t == 0 || changes.Count < t =>
                    changes.Select(change => new NotifyCollectionChangedEventArgs(
                                       NotifyCollectionChangedAction.Replace, change.New, change.Old)
                    ),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },

            _ => []
        };

        newArgs.ForEach(a => handler.Invoke(this, a));
    });

    private CountedWeakRefWrapper<
        PropertyChangedEventHandler,
        EventHandler<CollectionUpdateEventArgs<T>>
    > PropertyChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        IEnumerable<PropertyChangedEventArgs> newArgs = args switch
        {
            ICollectionItemAddedEventArgs<T> or ICollectionItemRemovedEventArgs<T> =>
            [
                new(nameof(Count))
            ],

            _ => []
        };

        newArgs.ForEach(a => handler.Invoke(this, a));
    });
}