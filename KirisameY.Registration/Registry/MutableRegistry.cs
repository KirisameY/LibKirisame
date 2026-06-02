using System.Collections;
using System.Diagnostics.CodeAnalysis;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;

namespace KirisameY.Registration.Registry;

/// <summary>
///     MoltenRegister, i.e. common register before freezing.<br/>
///     Allows for registration &amp; alteration of items, and can be converted to a <see cref="FrozenRegistry{T}"/>.<br/>
///     It is recommended to use it as an intermediate object for creating a frozen registry.
/// </summary>
/// <param name="fallback"> Fallback function for items that are not registered </param>
/// <remarks>
///     Note that this register cannot be used after freezing.
/// </remarks>
/// <seealso cref="RegistryBuilder{T}"/>
public class MutableRegistry<T>(Func<RegKey, T> fallback) : IRegisterTarget<T>, IEnumerableRegistry<T>
{
    private bool _frozen = false;

    private readonly Dictionary<RegKey, T> _regDict = new();

    public bool IsAvailableToRegister => !_frozen;

    public bool AddItem(RegKey key, T item)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        return _regDict.TryAdd(key, item);
    }

    public void AddOrOverwriteItem(RegKey key, T item)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        _regDict[key] = item;
    }

    public T GetItem(RegKey key)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        if (!_regDict.TryGetValue(key, out var value))
        {
            try { value = fallback(key); }
            catch (Exception e)
            {
                throw new GettingFallbackValueFailedException(this, key, e);
            }
        }

        return value;
    }

    public bool ItemRegistered(RegKey key)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        return _regDict.ContainsKey(key);
    }


    /// <summary>
    ///     Freeze current molten register to a frozen register.
    /// </summary>
    /// <returns> The new FrozenRegister. </returns>
    /// <exception cref="RegisterTargetUnavailableException"> Register is already frozen. </exception>
    /// <remarks>
    ///     The frozen register will be a new instance, and after freezing this instance will no more available and will throw an exception when used<br/>
    ///     Be sure to save the return value and discard this reference.
    /// </remarks>
    public FrozenRegistry<T> Freeze()
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        _frozen = true;
        return new FrozenRegistry<T>(_regDict, fallback);
    }


    public IEnumerator<KeyValuePair<RegKey, T>> GetEnumerator()
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        return _regDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _regDict.Count;
    public bool TryGetValue(RegKey key, [MaybeNullWhen(false)] out T value)
    {
        throw new NotImplementedException();
    }

    public T this[RegKey key] => throw new NotImplementedException();
    public IEnumerable<RegKey> Keys => _regDict.Keys;
    public IEnumerable<T> Values => _regDict.Values;
}

