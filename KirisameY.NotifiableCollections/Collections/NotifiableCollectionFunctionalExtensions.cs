using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     <see cref="INotifiableCollection{T}"/> 的功能性拓展方法：
///     <br/>
///     Functional extension methods for <see cref="INotifiableCollection{T}"/>.
/// </summary>
public static class NotifiableCollectionFunctionalExtensions
{
    extension<T>(INotifiableCollection<T> collection)
    {
        /// <inheritdoc cref="INotifiableCollection{T}.AddRange"/>
        [PublicAPI] public void AddRange(IEnumerable<T> items) => collection.AddRange([..items]);
    }
}