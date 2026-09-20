using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

// notifyThreshold：决定集合更新时应分多次发出单项通知还是一次性发出 Reset 通知
//     > 【非负值】：在发生变化的集合项数量大于该值时将发送单次 Reset（若为0则始终发出单次 Reset）
//     > 【　负值】：始终发出单次通知
internal class ObservableCollectionWrapper<T>(IReadOnlyNotifiableCollection<T> source, int notifyThreshold) : IReadOnlyObservableCollection<T>
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
                var (t, items) when t < 0 || items.Count <= t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            ICollectionItemRemovedEventArgs<T> removed => (notifyThreshold, removed.RemovedItems) switch
            {
                var (t, items) when t < 0 || items.Count <= t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            ICollectionItemReplacedEventArgs<T> replaced => (notifyThreshold, replaced.ItemChanges) switch
            {
                var (t, changes) when t < 0 || changes.Count <= t =>
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