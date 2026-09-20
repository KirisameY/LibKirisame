using System.Collections;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews;

internal class NotifiableCollectionReadonlyView<T>(IReadOnlyNotifiableCollection<T> source) : IReadOnlyNotifiableCollection<T>
{
    public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Count;


    public event EventHandler<CollectionUpdateEventArgs<T>>? CollectionUpdated
    {
        add => source.CollectionUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => source.CollectionUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<CollectionUpdateEventArgs<T>>,
        EventHandler<CollectionUpdateEventArgs<T>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        CollectionUpdateEventArgs<T>? newArgs = args switch
        {
            ICollectionItemAddedEventArgs<T> added => new CollectionItemAddedEventArgs<T>(
                this, added.AddedItems
            ),
            ICollectionItemClearedEventArgs<T> cleared => new CollectionItemClearedEventArgs<T>(
                this, cleared.RemovedItems
            ),
            ICollectionItemRemovedEventArgs<T> removed => new CollectionItemRemovedEventArgs<T>(
                this, removed.RemovedItems
            ),
            ICollectionItemReplacedEventArgs<T> replaced => new CollectionItemReplacedEventArgs<T>(
                this, replaced.OldItems, replaced.NewItems
            ),

            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}

internal class NotifiableCollectionReadonlyView<TSource, TValue>(
    IReadOnlyNotifiableCollection<TSource> source, Func<TSource, TValue> valueSelector
) : IReadOnlyNotifiableCollection<TValue>
{
    public IEnumerator<TValue> GetEnumerator() => source.Select(valueSelector).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Count;


    public event EventHandler<CollectionUpdateEventArgs<TValue>>? CollectionUpdated
    {
        add => source.CollectionUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => source.CollectionUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<CollectionUpdateEventArgs<TValue>>,
        EventHandler<CollectionUpdateEventArgs<TSource>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        CollectionUpdateEventArgs<TValue>? newArgs = args switch
        {
            ICollectionItemAddedEventArgs<TSource> added => new CollectionItemAddedEventArgs<TValue>(
                this, [..added.AddedItems.Select(valueSelector)]
            ),
            ICollectionItemClearedEventArgs<TSource> cleared => new CollectionItemClearedEventArgs<TValue>(
                this, [..cleared.RemovedItems.Select(valueSelector)]
            ),
            ICollectionItemRemovedEventArgs<TSource> removed => new CollectionItemRemovedEventArgs<TValue>(
                this, [..removed.RemovedItems.Select(valueSelector)]
            ),
            ICollectionItemReplacedEventArgs<TSource> replaced => new CollectionItemReplacedEventArgs<TValue>(
                this, [..replaced.OldItems.Select(valueSelector)], [..replaced.NewItems.Select(valueSelector)]
            ),

            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}