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
            // Add
            {
                Action: NotifyCollectionChangedAction.Add,
                NewItems: { } newItems,
                NewStartingIndex: var startIndex
            } => new ListItemAddedEventArgs<TItem>(
                this, [..newItems.Cast<TItem>()],
                startIndex >= 0 ? startIndex : list.Count - newItems.Count
            ),
            // Remove
            {
                Action: NotifyCollectionChangedAction.Remove,
                OldItems: { } oldItems,
                OldStartingIndex: var startIndex
            } => new ListItemRemovedEventArgs<TItem>(
                this, [..oldItems.Cast<TItem>()],
                [..Enumerable.Range(startIndex >= 0 ? startIndex : list.Count, oldItems.Count)]
            ),
            // Replace
            {
                Action: NotifyCollectionChangedAction.Replace,
                OldItems: { } oldItems,
                NewItems: { } newItems,
                NewStartingIndex: >= 0 and var startIndex
            } => new ListItemReplacedEventArgs<TItem>(
                this, [..oldItems.Cast<TItem>()], [..newItems.Cast<TItem>()],
                [..Enumerable.Range(startIndex, oldItems.Count)]
            ),
            // Move
            {
                Action: NotifyCollectionChangedAction.Move,
                OldItems: [{ } item],
                OldStartingIndex: >= 0 and var oldIndex,
                NewStartingIndex: >= 0 and var newIndex
            } => new ListItemMovedEventArgs<TItem>(
                this, [(TItem)item], [oldIndex], [newIndex]
            ),
            // Reset
            {
                Action: NotifyCollectionChangedAction.Reset
            } => new ListResetEventArgs<TItem>(this),
            // ERROR
            _ => throw new NotSupportedException($"Not supported collection changed event: {args}")
        };

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}