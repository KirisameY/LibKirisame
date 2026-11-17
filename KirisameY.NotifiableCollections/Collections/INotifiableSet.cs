using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可读写、且可以订阅变更通知的无重复集合。
///     <br/>
///     A mutable collection with no duplicated elements whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INotifiableSet<T> : INotifiableCollection<T>, ISet<T>, IReadOnlyNotifiableSet<T>
{
    #region Reading

    /// <summary>
    ///     判断该集合是否包含指定的元素。
    ///     <br/>
    ///     Determines if the set contains a specific element.
    /// </summary>
    [PublicAPI] new bool Contains(T element);

    bool IReadOnlySet<T>.Contains(T item) => Contains(item);
    bool ICollection<T>.Contains(T item) => Contains(item);


    /// <inheritdoc cref="INotifiableCollection{T}.Count" />
    [PublicAPI] new int Count { get; }

    int INotifiableCollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;
    int ICollection<T>.Count => Count;


    /// <summary>
    ///     判断当前集合是否为另一集合的子集。
    ///     <br/>
    ///     Determines whether the current set is a subset of a specified collection.
    /// </summary>
    new bool IsSubsetOf(IEnumerable<T> other);

    bool IReadOnlySet<T>.IsSubsetOf(IEnumerable<T> other) => IsSubsetOf(other);
    bool ISet<T>.IsSubsetOf(IEnumerable<T> other) => IsSubsetOf(other);

    /// <summary>
    ///     判断当前集合是否为另一集合的超集。
    ///     <br/>
    ///     Determines whether the current set is a superset of a specified collection.
    /// </summary>
    new bool IsSupersetOf(IEnumerable<T> other);

    bool IReadOnlySet<T>.IsSupersetOf(IEnumerable<T> other) => IsSupersetOf(other);
    bool ISet<T>.IsSupersetOf(IEnumerable<T> other) => IsSupersetOf(other);

    /// <summary>
    ///     判断当前集合是否为另一集合的真子集。
    ///     <br/>
    ///     Determines whether the current set is a proper (strict) subset of a specified collection.
    /// </summary>
    new bool IsProperSubsetOf(IEnumerable<T> other);

    bool IReadOnlySet<T>.IsProperSubsetOf(IEnumerable<T> other) => IsProperSubsetOf(other);
    bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other) => IsProperSubsetOf(other);

    /// <summary>
    ///     判断当前集合是否为另一集合的真超集。
    ///     <br/>
    ///     Determines whether the current set is a proper (strict) superset of a specified collection.
    /// </summary>
    new bool IsProperSupersetOf(IEnumerable<T> other);

    bool IReadOnlySet<T>.IsProperSupersetOf(IEnumerable<T> other) => IsProperSupersetOf(other);
    bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other) => IsProperSupersetOf(other);

    /// <summary>
    ///     判断当前集合是否与另一集合存在交集。
    ///     <br/>
    ///     Determines whether the current set overlaps with the specified collection.
    /// </summary>
    new bool Overlaps(IEnumerable<T> other);

    bool IReadOnlySet<T>.Overlaps(IEnumerable<T> other) => Overlaps(other);
    bool ISet<T>.Overlaps(IEnumerable<T> other) => Overlaps(other);

    /// <summary>
    ///     判断当前集合是否与另一集合包含完全相同的元素。
    ///     <br/>
    ///     Determines whether the current set and the specified collection contain the same elements.
    /// </summary>
    new bool SetEquals(IEnumerable<T> other);

    bool IReadOnlySet<T>.SetEquals(IEnumerable<T> other) => SetEquals(other);
    bool ISet<T>.SetEquals(IEnumerable<T> other) => SetEquals(other);

    #endregion


    #region Updating

    /// <summary>
    ///     尝试向集合中加入一个元素。
    ///     <br/>
    ///     Try to add an element to the set.
    /// </summary>
    /// <returns>
    ///     当元素被成功加入时为 <c>true</c>，当元素已存在时为 <c>false</c>。
    ///     <br/>
    ///     <c>true</c> if element added to the set successfully,
    ///     <c>false</c> if element already exists.
    /// </returns>
    [PublicAPI] new bool Add(T element);

    bool ISet<T>.Add(T item) => Add(item);
    void ICollection<T>.Add(T item) => Add(item);


    /// <summary>
    ///     尝试向集合中加入一系列元素。
    ///     <br/>
    ///     Try to add elements to the set.
    /// </summary>
    /// <returns>
    ///     被成功加入的元素。
    ///     <br/>
    ///     Elements that added to the set successfully.
    /// </returns>
    [PublicAPI] IReadOnlySet<T> UnionWith(IReadOnlyCollection<T> elements);

    void INotifiableCollection<T>.AddRange(IReadOnlyCollection<T> elements) => UnionWith(elements);
    void ISet<T>.UnionWith(IEnumerable<T> other) => UnionWith([..other]);

    /// <inheritdoc cref="INotifiableCollection{T}.RemoveRange" />
    [PublicAPI] IReadOnlySet<T> ExceptWith(IReadOnlyCollection<T> elements);

    IReadOnlyCollection<T> INotifiableCollection<T>.RemoveRange(IReadOnlyCollection<T> elements) => ExceptWith(elements);
    void ISet<T>.ExceptWith(IEnumerable<T> other) => ExceptWith(other);


    /// <summary>
    ///
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    [PublicAPI] new IReadOnlySet<T> IntersectWith(IEnumerable<T> other);

    void ISet<T>.IntersectWith(IEnumerable<T> other) => IntersectWith(other);

    /// <summary>
    ///     修改当前集合，使其仅包含仅被当前集合活另一集合包含的元素，而不包含两者共同包含的元素。
    ///     <br/>
    ///     Modifies the current set so that it contains only elements that are present
    ///     either in the current set or in the specified collection, but not both.
    /// </summary>
    /// <returns>
    ///     被向当前集合中加入和从当前集合中移除的元素。
    ///     <br/>
    ///     Elements that added to or removed from the current set.
    /// </returns>
    [PublicAPI] new (IReadOnlySet<T> Added, IReadOnlySet<T> Removed) SymmetricExceptWith(IEnumerable<T> other);

    void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => SymmetricExceptWith(other);


    /// <inheritdoc cref="INotifiableCollection{T}.RemoveAll" />
    [PublicAPI] new IReadOnlySet<T> RemoveAll(Predicate<T> predicate);

    IReadOnlyCollection<T> INotifiableCollection<T>.RemoveAll(Predicate<T> predicate) => RemoveAll(predicate);

    #endregion
}