using System.Collections;
using System.Collections.Frozen;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Utils;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews;

internal class NotifiableSetReadOnlyView<T>(IReadOnlyNotifiableSet<T> set) : IReadOnlyNotifiableSet<T>
{
    public IEnumerator<T> GetEnumerator() => set.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => set.Count;
    public bool Contains(T item) => set.Contains(item);
    public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);
    public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);


    public event EventHandler<SetUpdateEventArgs<T>>? SetUpdated
    {
        add => set.SetUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => set.SetUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<SetUpdateEventArgs<T>>,
        EventHandler<SetUpdateEventArgs<T>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        SetUpdateEventArgs<T>? newArgs = args switch
        {
            ISetElementAddedEventArgs<T> added => new SetElementAddedEventArgs<T>(
                this, added.AddedElements
            ),
            ISetElementClearedEventArgs<T> cleared => new SetElementClearedEventArgs<T>(
                this, cleared.RemovedElements
            ),
            ISetElementRemovedByReplaceEventArgs<T> removedByReplace => new SetElementRemovedByReplaceEventArgs<T>(
                this, removedByReplace.ReplacePairs
            ),
            ISetElementRemovedEventArgs<T> removed => new SetElementRemovedEventArgs<T>(
                this, removed.RemovedElements
            ),
            ISetElementReplacedEventArgs<T> replaced => new SetElementReplacedEventArgs<T>(
                this, replaced.OldItems, replaced.NewItems
            ),
            ISetResetEventArgs<T> => new SetResetEventArgs<T>(this),

            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}

internal class NotifiableSetReadOnlyView<TSource, TValue>(
    IReadOnlyNotifiableSet<TSource> set, Func<TSource, TValue> valueSelector, Func<TValue, Optional<TSource>> valueReserveSelector
) : IReadOnlyNotifiableSet<TValue>
{
    public IEnumerator<TValue> GetEnumerator() => set.Select(valueSelector).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => set.Count;

    public bool Contains(TValue item) =>
        valueReserveSelector.Invoke(item) is { HasValue: true, Value: var value }
     && set.Contains(value);

    public bool IsProperSubsetOf(IEnumerable<TValue> other) => set.IsProperSubsetOf(
        other.Select(valueReserveSelector)
             .Where(opt => opt.HasValue)
             .Select(opt => opt.Value)
    );

    public bool IsProperSupersetOf(IEnumerable<TValue> other) =>
        other.Select(valueReserveSelector).ToArray() is var array
     && array.All(opt => opt.HasValue)
     && set.IsProperSupersetOf(array.Select(opt => opt.Value));

    public bool IsSubsetOf(IEnumerable<TValue> other) => set.IsSubsetOf(
        other.Select(valueReserveSelector)
             .Where(opt => opt.HasValue)
             .Select(opt => opt.Value)
    );

    public bool IsSupersetOf(IEnumerable<TValue> other) =>
        other.Select(valueReserveSelector).ToArray() is var array
     && array.All(opt => opt.HasValue)
     && set.IsSupersetOf(array.Select(opt => opt.Value));

    public bool Overlaps(IEnumerable<TValue> other) => set.Overlaps(
        other.Select(valueReserveSelector)
             .Where(opt => opt.HasValue)
             .Select(opt => opt.Value)
    );

    public bool SetEquals(IEnumerable<TValue> other) =>
        other.Select(valueReserveSelector).ToArray() is var array
     && array.All(opt => opt.HasValue)
     && set.SetEquals(array.Select(opt => opt.Value));


    public event EventHandler<SetUpdateEventArgs<TValue>>? SetUpdated
    {
        add => set.SetUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => set.SetUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<SetUpdateEventArgs<TValue>>,
        EventHandler<SetUpdateEventArgs<TSource>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        SetUpdateEventArgs<TValue>? newArgs = args switch
        {
            ISetElementAddedEventArgs<TSource> added => new SetElementAddedEventArgs<TValue>(
                this, added.AddedElements.Select(valueSelector).ToFrozenSet()
            ),
            ISetElementClearedEventArgs<TSource> cleared => new SetElementClearedEventArgs<TValue>(
                this, cleared.RemovedElements.Select(valueSelector).ToFrozenSet()
            ),
            ISetElementRemovedByReplaceEventArgs<TSource> removedByReplace => new SetElementRemovedByReplaceEventArgs<TValue>(
                this, NullableFrozenDictionary.From(
                    removedByReplace.ReplacePairs.Select(pair => (valueSelector.Invoke(pair.Key), valueSelector.Invoke(pair.Value)))
                )
            ),
            ISetElementRemovedEventArgs<TSource> removed => new SetElementRemovedEventArgs<TValue>(
                this, removed.RemovedElements.Select(valueSelector).ToFrozenSet()
            ),
            ISetElementReplacedEventArgs<TSource> replaced => new SetElementReplacedEventArgs<TValue>(
                this, [..replaced.OldItems.Select(valueSelector)], [..replaced.NewItems.Select(valueSelector)]
            ),
            ISetResetEventArgs<TSource> => new SetResetEventArgs<TValue>(this),

            _ => null
        };

        if (newArgs is not null) handler.Invoke(this, newArgs);
    });
}