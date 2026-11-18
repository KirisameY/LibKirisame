namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     <see cref="INotifiableSet{T}"/> 的默认实现。
///     <br/>
///     Default implement of <see cref="INotifiableSet{T}"/>.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
public class NotifiableHashSet<T>(int capacity = 0, IEqualityComparer<T>? comparer = null) : NotifiableSetSimpleBase<T>
{
    private readonly HashSet<T> _innerSet = new(capacity, comparer);

    /// <inheritdoc />
    protected override IReadOnlySet<T> View => field ??= _innerSet.AsReadOnly();

    /// <inheritdoc />
    protected override bool AddElement(T element) => _innerSet.Add(element);

    /// <inheritdoc />
    protected override bool RemoveElement(T element) => _innerSet.Remove(element);

    /// <inheritdoc />
    protected override void ClearElements() => _innerSet.Clear();

    /// <inheritdoc />
    protected override IEqualityComparer<T> EqualityComparer => comparer ?? EqualityComparer<T>.Default;
}