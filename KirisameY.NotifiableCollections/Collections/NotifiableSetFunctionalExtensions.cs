using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     <see cref="INotifiableSet{T}"/> 的功能性拓展方法：
///     <br/>
///     Functional extension methods for <see cref="INotifiableSet{T}"/>.
/// </summary>
public static class NotifiableSetFunctionalExtensions
{
    extension<T>(INotifiableSet<T> set)
    {
        /// <inheritdoc cref="INotifiableSet{T}.UnionWith(IReadOnlyCollection{T})"/>
        [PublicAPI] public IReadOnlyCollection<T> UnionWith(IEnumerable<T> items) => set.UnionWith([..items]);

        /// <inheritdoc cref="INotifiableSet{T}.ExceptWith(IReadOnlyCollection{T})"/>
        [PublicAPI] public IReadOnlyCollection<T> ExceptWith(IEnumerable<T> items) => set.ExceptWith([..items]);
    }
}