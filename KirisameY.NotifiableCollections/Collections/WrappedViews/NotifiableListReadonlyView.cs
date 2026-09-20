using System.Collections;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews;

internal class NotifiableListReadonlyView<T>(IReadOnlyNotifiableList<T> list) : IReadOnlyNotifiableList<T>
{
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => list.Count;
    public T this[int index] => list[index];


    public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated
    {
        add => list.ListUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => list.ListUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<ListUpdateEventArgs<T>>,
        EventHandler<ListUpdateEventArgs<T>>
    > HandlerCache => field ??= new(handler => (_, args) => handler.Invoke(this, args));
}

internal class NotifiableListReadonlyView<TSource, TValue>(
    IReadOnlyNotifiableList<TSource> list, Func<TSource, TValue> valueSelector
) : IReadOnlyNotifiableList<TValue>
{
    public IEnumerator<TValue> GetEnumerator() => list.Select(valueSelector).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => list.Count;
    public TValue this[int index] => valueSelector.Invoke(list[index]);


    public event EventHandler<ListUpdateEventArgs<TValue>>? ListUpdated
    {
        add => list.ListUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => list.ListUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<ListUpdateEventArgs<TValue>>,
        EventHandler<ListUpdateEventArgs<TSource>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        ListUpdateEventArgs<TValue> newArgs = args switch
        {
            IListItemAddedEventArgs<TSource> added => new ListItemAddedEventArgs<TValue>(
                this, [..added.AddedItems.Select(valueSelector)], added.StartIndex
            ),
            IListItemClearedEventArgs<TSource> cleared => new ListItemClearedEventArgs<TValue>(
                this, [..cleared.RemovedItems.Select(valueSelector)], cleared.Indexes
            ),
            IListItemRemovedEventArgs<TSource> removed => new ListItemRemovedEventArgs<TValue>(
                this, [..removed.RemovedItems.Select(valueSelector)], removed.Indexes
            ),
            IListItemReplacedEventArgs<TSource> replaced => new ListItemReplacedEventArgs<TValue>(
                this, [..replaced.OldItems.Select(valueSelector)],
                [..replaced.NewItems.Select(valueSelector)], replaced.Indexes
            ),
            IListSortedEventArgs<TSource> => new ListSortedEventArgs<TValue>(this),

            _ => throw new InvalidDataException($"Unexpected type {args.GetType()} of args")
        };

        handler.Invoke(this, newArgs);
    });
}