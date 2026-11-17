using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using JetBrains.Annotations;

using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Utils;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     用于简易实现 <see cref="INotifiableSet{T}"/> 的基类型。
///     <br/>
///     Base type for implement <see cref="INotifiableSet{T}"/> easily.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public abstract class NotifiableSetSimpleBase<T> : INotifiableSet<T>
{
    #region Abstract & Virtual

    /// <summary>
    ///     该列表的只读包装视图。
    ///     <br/>
    ///     Read-only wrapped view of this list.
    /// </summary>
    protected abstract IReadOnlySet<T> View { get; }

    /// <summary>
    ///     尝试向集合中添加一个元素。
    ///     <br/>
    ///     Try to insert an element into the set.
    /// </summary>
    protected abstract bool AddElement(T element);

    /// <summary>
    ///     尝试从集合中移除一个元素。
    ///     <br/>
    ///     Try to remove an element from the set.
    /// </summary>
    protected abstract bool RemoveElement(T element);

    /// <summary>
    ///     移除集合里的所有元素。
    ///     <br/>
    ///     Remove all elements in the set.
    /// </summary>
    protected abstract void ClearElements();

    /// <summary>
    ///     用于比较元素相等性的比较器。
    ///     <br/>
    ///     Comparer that used to compare elements in the set.
    /// </summary>
    protected abstract IEqualityComparer<T> EqualityComparer { get; }

    #endregion


    #region Reading

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => View.GetEnumerator();

    /// <inheritdoc cref="INotifiableCollection{T}.Count" />
    public int Count => View.Count;

    /// <inheritdoc cref="INotifiableSet{T}.Contains" />
    public bool Contains(T item) => View.Contains(item);

    /// <inheritdoc cref="INotifiableSet{T}.IsSubsetOf" />
    public bool IsSubsetOf(IEnumerable<T> other) => View.IsSubsetOf(other);

    /// <inheritdoc cref="INotifiableSet{T}.IsSupersetOf" />
    public bool IsSupersetOf(IEnumerable<T> other) => View.IsSupersetOf(other);

    /// <inheritdoc cref="INotifiableSet{T}.IsProperSubsetOf"/>
    public bool IsProperSubsetOf(IEnumerable<T> other) => View.IsProperSubsetOf(other);

    /// <inheritdoc cref="INotifiableSet{T}.IsProperSupersetOf" />
    public bool IsProperSupersetOf(IEnumerable<T> other) => View.IsProperSupersetOf(other);

    /// <inheritdoc cref="INotifiableSet{T}.Overlaps" />
    public bool Overlaps(IEnumerable<T> other) => View.Overlaps(other);

    /// <inheritdoc cref="INotifiableSet{T}.SetEquals" />
    public bool SetEquals(IEnumerable<T> other) => View.SetEquals(other);

    #endregion


    #region Writing

    /// <inheritdoc cref="INotifiableCollection{T}.Add" />
    public bool Add(T element)
    {
        var result = AddElement(element);
        if (result) OnSetUpdated(new SetElementAddedEventArgs<T>(View, FrozenSet.Create(element)));
        return result;
    }

    /// <inheritdoc />
    public bool Remove(T element)
    {
        if (!RemoveElement(element)) return false;
        OnSetUpdated(new SetElementRemovedEventArgs<T>(View, FrozenSet.Create(element)));
        return true;
    }

    /// <inheritdoc />
    public IReadOnlySet<T> UnionWith(IReadOnlyCollection<T> elements)
    {
        List<T> added = new(capacity: elements.Count);
        foreach (var element in elements)
        {
            if (AddElement(element)) added.Add(element);
        }

        if (added.Count == 0) return ReadOnlySet<T>.Empty;
        var finalAdded = added.ToFrozenSet();
        OnSetUpdated(new SetElementAddedEventArgs<T>(View, finalAdded));
        return finalAdded;
    }

    /// <inheritdoc />
    public IReadOnlySet<T> ExceptWith(IReadOnlyCollection<T> elements)
    {
        var removed = elements.Where(RemoveElement).ToFrozenSet(comparer: EqualityComparer);
        if (removed.Count > 0) OnSetUpdated(new SetElementRemovedEventArgs<T>(View, removed));
        return removed;
    }

    /// <inheritdoc />
    public IReadOnlySet<T> IntersectWith(IEnumerable<T> other)
    {
        var toRemove = View.ToHashSet(comparer: EqualityComparer);
        toRemove.ExceptWith(other);
        return ExceptWith(toRemove);
    }

    /// <inheritdoc />
    public (IReadOnlySet<T> Added, IReadOnlySet<T> Removed) SymmetricExceptWith(IEnumerable<T> other)
    {
        HashSet<T> added = new(comparer: EqualityComparer),
                   removed = new(comparer: EqualityComparer);
        foreach (var element in other.Distinct(comparer: EqualityComparer))
        {
            if (AddElement(element))
            {
                added.Add(element);
            }
            else
            {
                RemoveElement(element);
                removed.Add(element);
            }
        }

        var finalAdded = added.ToFrozenSet();
        var finalRemoved = removed.ToFrozenSet();

        if (finalAdded.Count > 0) OnSetUpdated(new SetElementAddedEventArgs<T>(View, finalAdded));
        if (finalRemoved.Count > 0) OnSetUpdated(new SetElementRemovedEventArgs<T>(View, finalRemoved));

        return (finalAdded, finalRemoved);
    }

    /// <inheritdoc />
    public IReadOnlySet<T> RemoveAll(Predicate<T> predicate) => ExceptWith(View.Where(predicate.Invoke).ToArray());

    /// <inheritdoc />
    public bool Replace(T oldElement, T newElement)
    {
        if (!RemoveElement(oldElement)) return false;
        if (!AddElement(newElement))
            OnSetUpdated(new SetElementRemovedByReplaceEventArgs<T>(View, NullableFrozenDictionary.From((oldElement, newElement))));
        else
            OnSetUpdated(new SetElementReplacedEventArgs<T>(View, [oldElement], [newElement]));
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        var snapshot = View.ToFrozenSet();
        ClearElements();
        OnSetUpdated(new SetElementClearedEventArgs<T>(View, snapshot));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<(bool Replaced, T Left)> ReplaceRange(IReadOnlyCollection<(T Old, T New)> replaces)
    {
        List<(bool Replaced, T Left)> result = new(capacity: replaces.Count);
        List<(T Old, T New)> replaced = new(capacity: replaces.Count);
        List<(T Old, T New)> removed = new(capacity: replaces.Count);

        foreach (var (oldElement, newElement) in replaces)
        {
            if (!RemoveElement(oldElement)) result.Add((false, newElement));
            else
            {
                result.Add((true, oldElement));
                if (AddElement(newElement)) replaced.Add((oldElement, newElement));
                else removed.Add((oldElement, newElement));
            }
        }

        if (removed.Count > 0) OnSetUpdated(new SetElementRemovedByReplaceEventArgs<T>(View, NullableFrozenDictionary.From(removed)));
        if (replaced.Count > 0)
        {
            OnSetUpdated(
                new SetElementReplacedEventArgs<T>(
                    View, [..replaced.Select(t => t.Old)], [..replaced.Select(t => t.New)]
                )
            );
        }

        return [..result];
    }

    #endregion


    #region Event

    /// <summary>
    ///     引发 <see cref="SetUpdated"/> 事件。
    ///     <br/>
    ///     Raise the <see cref="SetUpdated"/> event.
    /// </summary>
    /// <remarks>
    ///     覆写该方法时，应当调用 <c>base.OnSetUpdated(e)</c> 以保证事件处理器能够接收到通知。
    ///     <br/>
    ///     When overriding, it's recommended to call <c>base.OnSetUpdated(e)</c>
    ///     to ensure that event handlers can receive the notification.
    /// </remarks>
    [PublicAPI] protected virtual void OnSetUpdated(SetUpdateEventArgs<T> args)
    {
        RaiseUpdate(args);
    }

    private ImmutableList<EventHandler<SetUpdateEventArgs<T>>> _updatedEventHandlers = [];

    private void RaiseUpdate(SetUpdateEventArgs<T> args)
    {
        foreach (var eventHandler in _updatedEventHandlers) eventHandler.Invoke(this, args);
    }

    /// <inheritdoc/>
    public event EventHandler<SetUpdateEventArgs<T>> SetUpdated
    {
        add => _updatedEventHandlers = _updatedEventHandlers.Add(value);
        remove => _updatedEventHandlers = _updatedEventHandlers.Remove(value);
    }

    #endregion
}