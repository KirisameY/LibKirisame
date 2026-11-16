using System.Collections.Immutable;
using System.Collections.ObjectModel;

using JetBrains.Annotations;

using KirisameY.NotifiableCollections.EventArgs;

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
    protected abstract IReadOnlyCollection<T> View { get; }

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

    #endregion


    #region Reading

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => View.GetEnumerator();

    /// <inheritdoc cref="INotifiableCollection{T}.Count" />
    public int Count => View.Count;

    /// <inheritdoc />
    public bool Contains(T item) => View.Contains(item);

    #endregion


    #region Writing

    /// <inheritdoc />
    public bool TryAdd(T element)
    {
        var result = AddElement(element);
        if (result) OnCollectionUpdated(new CollectionItemAddedEventArgs<T>(View, [element]));
        return result;
    }

    /// <inheritdoc/>
    void ICollection<T>.Add(T element) => TryAdd(element);

    /// <inheritdoc />
    public IReadOnlyCollection<T> TryAddRange(IReadOnlyCollection<T> elements)
    {
        List<T> added = new(capacity: elements.Count);
        foreach (var element in elements)
        {
            if (AddElement(element)) added.Add(element);
        }

        if (added.Count == 0) return ReadOnlyCollection<T>.Empty;
        var finalAdded = added.ToImmutableList();
        OnCollectionUpdated(new CollectionItemAddedEventArgs<T>(View, finalAdded));
        return finalAdded;
    }

    /// <inheritdoc />
    void INotifiableCollection<T>.AddRange(IReadOnlyCollection<T> elements) => TryAddRange(elements);

    /// <inheritdoc />
    public bool Remove(T element)
    {
        if (!RemoveElement(element)) return false;
        OnCollectionUpdated(new CollectionItemRemovedEventArgs<T>(View, [element]));
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<T> RemoveRange(IReadOnlyCollection<T> elements)
    {
        var removed = elements.Where(RemoveElement).ToImmutableList();
        if (removed.Count > 0) OnCollectionUpdated(new CollectionItemRemovedEventArgs<T>(View, removed));
        return removed;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<T> RemoveAll(Predicate<T> predicate) => RemoveRange(View.Where(predicate.Invoke).ToArray());

    /// <inheritdoc />
    public bool Replace(T oldElement, T newElement)
    {
        if (!RemoveElement(oldElement)) return false;
        if (!AddElement(newElement)) OnCollectionUpdated(new CollectionItemRemovedEventArgs<T>(View, [oldElement]));
        else OnCollectionUpdated(new CollectionItemReplacedEventArgs<T>(View, [oldElement], [newElement]));
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<(bool Replaced, T Left)> ReplaceRange(IReadOnlyCollection<(T Old, T New)> replaces)
    {
        List<(bool Replaced, T Left)> result = new(capacity: replaces.Count);
        List<(T Old, T New)> replaced = new(capacity: replaces.Count);
        List<T> removed = new(capacity: replaces.Count);

        foreach (var (oldElement, newElement) in replaces)
        {
            if (!RemoveElement(oldElement)) result.Add((false, newElement));
            else
            {
                result.Add((true, oldElement));
                if (AddElement(newElement)) replaced.Add((oldElement, newElement));
                else removed.Add(oldElement);
            }
        }

        if (removed.Count > 0) OnCollectionUpdated(new CollectionItemRemovedEventArgs<T>(View, [..removed]));
        if (replaced.Count > 0)
        {
            OnCollectionUpdated(
                new CollectionItemReplacedEventArgs<T>(
                    View, [..replaced.Select(t => t.Old)], [..replaced.Select(t => t.New)]
                )
            );
        }

        return [..result];
    }

    /// <inheritdoc />
    public void Clear()
    {
        ImmutableList<T> snapshot = [..View];
        ClearElements();
        OnCollectionUpdated(new CollectionItemClearedEventArgs<T>(View, snapshot));
    }

    #endregion


    #region Event

    /// <summary>
    ///     引发 <see cref="CollectionUpdated"/> 事件。
    ///     <br/>
    ///     Raise the <see cref="CollectionUpdated"/> event.
    /// </summary>
    /// <remarks>
    ///     覆写该方法时，应当调用 <c>base.OnCollectionUpdated(e)</c> 以保证事件处理器能够接收到通知。
    ///     <br/>
    ///     When overriding, it's recommended to call <c>base.OnCollectionUpdated(e)</c>
    ///     to ensure that event handlers can receive the notification.
    /// </remarks>
    [PublicAPI] protected virtual void OnCollectionUpdated(CollectionUpdateEventArgs<T> args)
    {
        RaiseUpdate(args);
    }

    private ImmutableList<EventHandler<CollectionUpdateEventArgs<T>>> _updatedEventHandlers = [];

    private void RaiseUpdate(CollectionUpdateEventArgs<T> args)
    {
        foreach (var eventHandler in _updatedEventHandlers) eventHandler.Invoke(this, args);
    }

    /// <inheritdoc/>
    public event EventHandler<CollectionUpdateEventArgs<T>> CollectionUpdated
    {
        add => _updatedEventHandlers = _updatedEventHandlers.Add(value);
        remove => _updatedEventHandlers = _updatedEventHandlers.Remove(value);
    }

    #endregion
}