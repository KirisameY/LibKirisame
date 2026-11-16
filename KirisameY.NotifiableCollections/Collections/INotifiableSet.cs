using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     可读写、且可以订阅变更通知的无重复集合。
///     <br/>
///     A mutable collection with no duplicated elements whose changes can be subscribed to.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INotifiableSet<T> : INotifiableCollection<T>
{
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
    [PublicAPI] bool TryAdd(T element);

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
    [PublicAPI] IReadOnlyCollection<T> TryAddRange(IReadOnlyCollection<T> elements);
}