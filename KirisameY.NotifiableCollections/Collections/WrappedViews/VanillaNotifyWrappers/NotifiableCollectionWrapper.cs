using System.Collections;
using System.Collections.Specialized;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

internal class NotifiableCollectionWrapper<TElement, TCollection>(TCollection collection) : IReadOnlyNotifiableCollection<TElement>
    where TCollection : IReadOnlyCollection<TElement>, INotifyCollectionChanged
{
    public IEnumerator<TElement> GetEnumerator() => collection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => collection.Count;


    // event
    public event EventHandler<CollectionUpdateEventArgs<TElement>>? CollectionUpdated
    {
        add => collection.CollectionChanged += value is null ? null : PropertyChangedHandlerCache.WrapNew(value);
        remove => collection.CollectionChanged -= value is null ? null : PropertyChangedHandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<CollectionUpdateEventArgs<TElement>>,
        NotifyCollectionChangedEventHandler
    > PropertyChangedHandlerCache => field ??= new(handler => (_, args) =>
    {
        CollectionUpdateEventArgs<TElement>? newArgs = args switch
        {
            {
                Action: NotifyCollectionChangedAction.Add,
                NewItems: { } newItems
            } => new CollectionItemAddedEventArgs<TElement>(
                this, [..newItems.Cast<TElement>()]
            ),
            {
                Action: NotifyCollectionChangedAction.Remove,
                OldItems: { } oldItems
            } => new CollectionItemRemovedEventArgs<TElement>(
                this, [..oldItems.Cast<TElement>()]
            ),
            {
                Action: NotifyCollectionChangedAction.Replace,
                OldItems: { } oldItems,
                NewItems: { } newItems
            } => new CollectionItemReplacedEventArgs<TElement>(
                this, [..oldItems.Cast<TElement>()], [..newItems.Cast<TElement>()]
            ),
            {
                Action: NotifyCollectionChangedAction.Reset
            } => new CollectionResetEventArgs<TElement>(this),
            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}