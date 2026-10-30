using KirisameY.Registration.Data;

namespace KirisameY.Registration.Registry;

public interface IRegistry
{
    /// <summary>
    ///     Check if given id is registered.
    /// </summary>
    bool ItemRegistered(RegKey key);
}

public interface IRegistry<out T> : IRegistry
{
    /// <summary>
    ///     Get registered item by id.
    /// </summary>
    /// <remarks>
    ///     Note that when implement this method, it should not throw exception when given id is not registered.
    ///     Instead, consider a default value or fallback delegate.
    /// </remarks>
    T GetItem(RegKey key) => GetItem(key, out _);

    /// <inheritdoc cref="GetItem(RegKey)"/>
    protected internal T GetItem(RegKey key, out bool succeed);
}

/// <summary>
///     Readable registry interface that can be iterated as a dictionary.
/// </summary>
public interface IEnumerableRegistry<T> : IRegistry<T>, IReadOnlyDictionary<RegKey, T>
{
    bool IReadOnlyDictionary<RegKey, T>.ContainsKey(RegKey key) => ItemRegistered(key);
}