using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using KirisameY.Registration.Data;

namespace KirisameY.Registration.Registry;

public static class RegistryExtensions
{
    extension<T>(IRegistry<T> reg)
    {
        // todo: 等待 .Net 11
        // public T this[RegKey id] => null;

        [PublicAPI] public bool TryGetValue(RegKey key, out T value)
        {
            value = reg.GetItem(key, out var succeed);
            return succeed;
        }

        [return: NotNullIfNotNull(nameof(fallback))]
        [PublicAPI] public T? GetValueOrDefault(RegKey key, T? fallback) =>
            reg.TryGetValue(key, out var value) ? value : fallback;
    }
}