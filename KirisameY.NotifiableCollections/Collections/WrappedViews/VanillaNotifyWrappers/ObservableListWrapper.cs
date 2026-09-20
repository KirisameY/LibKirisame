using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

internal class ObservableListWrapper<T>(IReadOnlyNotifiableList<T> list, int notifyThreshold) : IReadOnlyObservableList<T>
{
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => list.Count;

    public T this[int index] => list[index];


    // Events

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => list.ListUpdated += value is null ? null : CollectionChangedHandlerCache.WrapNew(value);
        remove => list.ListUpdated -= value is null ? null : CollectionChangedHandlerCache.RemoveWrapped(value);
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => list.ListUpdated += value is null ? null : PropertyChangedHandlerCache.WrapNew(value);
        remove => list.ListUpdated -= value is null ? null : PropertyChangedHandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        NotifyCollectionChangedEventHandler,
        EventHandler<ListUpdateEventArgs<T>>
    > CollectionChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        IEnumerable<NotifyCollectionChangedEventArgs> newArgs = args switch
        {
            IListItemAddedEventArgs<T> added => (notifyThreshold, added.AddedItemsWithIndex) switch
            {
                var (t, items) when t < 0 || items.Count <= t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item.Item, item.Index)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            IListItemRemovedEventArgs<T> removed => (notifyThreshold, removed.RemovedItemsWithIndex) switch
            {
                var (t, items) when t < 0 || items.Count <= t =>
                    items.Select(item => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item.Item, item.Index)),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            IListItemReplacedEventArgs<T> replaced => (notifyThreshold, replaced.ItemChanges) switch
            {
                var (t, changes) when t < 0 || changes.Count <= t =>
                    changes.Select(change => new NotifyCollectionChangedEventArgs(
                                       NotifyCollectionChangedAction.Replace, change.New, change.Old, change.Index)
                    ),
                _ => [new(NotifyCollectionChangedAction.Reset)]
            },
            IListSortedEventArgs<T> => [new(NotifyCollectionChangedAction.Reset)],

            _ => []
        };

        newArgs.ForEach(a => handler.Invoke(this, a));
    });

    private CountedWeakRefWrapper<
        PropertyChangedEventHandler,
        EventHandler<ListUpdateEventArgs<T>>
    > PropertyChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        const string indexerName = "Item[]";

        IEnumerable<PropertyChangedEventArgs> newArgs = args switch
        {
            IListItemAddedEventArgs<T> or IListItemRemovedEventArgs<T> =>
            [
                new(nameof(Count)),
                new(indexerName)
            ],
            IListItemReplacedEventArgs<T> or IListSortedEventArgs<T> =>
            [
                new(indexerName)
            ],

            _ => []
        };

        newArgs.ForEach(a => handler.Invoke(this, a));
    });
}