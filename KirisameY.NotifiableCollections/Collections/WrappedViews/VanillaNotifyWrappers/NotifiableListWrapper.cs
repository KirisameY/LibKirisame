using System.Collections;
using System.Collections.Specialized;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

internal class NotifiableListWrapper<TItem, TList>(TList list) : IReadOnlyNotifiableList<TItem>
    where TList : IReadOnlyList<TItem>, INotifyCollectionChanged
{
    public IEnumerator<TItem> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => list.Count;
    public TItem this[int index] => list[index];


    // event
    public event EventHandler<ListUpdateEventArgs<TItem>>? ListUpdated
    {
        add => list.CollectionChanged += value is null ? null : PropertyChangedHandlerCache.WrapNew(value);
        remove => list.CollectionChanged -= value is null ? null : PropertyChangedHandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<ListUpdateEventArgs<TItem>>,
        NotifyCollectionChangedEventHandler
    > PropertyChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        ListUpdateEventArgs<TItem>? newArgs = args switch
        {
            {
                Action: NotifyCollectionChangedAction.Add,
                NewItems: { } newItems,
                NewStartingIndex: var startIndex
            } => new ListItemAddedEventArgs<TItem>(
                this, [..newItems.Cast<TItem>()], startIndex
            ),
            {
                Action: NotifyCollectionChangedAction.Remove,
                OldItems: { } oldItems,
                OldStartingIndex: var startIndex
            } => new ListItemRemovedEventArgs<TItem>(
                this, [..oldItems.Cast<TItem>()], [..Enumerable.Range(startIndex, oldItems.Count)]
            ),
            {
                Action: NotifyCollectionChangedAction.Replace,
                OldItems: { } oldItems,
                NewItems: { } newItems,
                NewStartingIndex: var startIndex
            } => new ListItemReplacedEventArgs<TItem>(
                this, [..oldItems.Cast<TItem>()], [..newItems.Cast<TItem>()],
                [..Enumerable.Range(startIndex, oldItems.Count)]
            ),
            {
                Action: NotifyCollectionChangedAction.Move,
                OldItems: [{ } item],
                OldStartingIndex: var oldIndex,
                NewStartingIndex: var newIndex
            } => new ListItemMovedEventArgs<TItem>(
                this, [(TItem)item], [oldIndex], [newIndex]
            ),
            {
                Action: NotifyCollectionChangedAction.Reset
            } => new ListResetEventArgs<TItem>(this),
            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}