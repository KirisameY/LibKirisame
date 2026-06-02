using System.Collections;
using System.Collections.Frozen;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;

namespace KirisameY.Registration.Registry;

/// <summary>
///     An immutable register.<br/>
///     The most common implementation of <see cref="IRegistry{T}"/>.
/// </summary>
/// <param name="regDict">
///     Source dictionary that contains all registered id-item pairs.<br/>
///     Will not be stored in the new instance.
/// </param>
/// <param name="fallback"> Fallback function for items that are not registered. </param>
/// <seealso cref="MutableRegistry{T}"/>
/// <seealso cref="RegistryBuilder{T}"/>
public class FrozenRegistry<T>(IDictionary<RegKey, T> regDict, Func<RegKey, T> fallback) : IEnumerableRegistry<T>
{
    private readonly FrozenDictionary<RegKey, T> _regDict = regDict.ToFrozenDictionary();

    public T this[RegKey key] => GetItem(key);
    public IEnumerable<RegKey> Keys => _regDict.Keys;
    public IEnumerable<T> Values => _regDict.Values;
    public int Count => _regDict.Count;

    public T GetItem(RegKey key)
    {
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

    public bool ItemRegistered(RegKey id) => _regDict.ContainsKey(id);

    public bool TryGetValue(RegKey key, out T value)
    {
        bool result = _regDict.TryGetValue(key, out var item);
        try
        {
            value = result ? item! : fallback(key);
            return result;
        }
        catch (Exception e)
        {
            throw new GettingFallbackValueFailedException(this, key, e);
        }
    }

    public IEnumerator<KeyValuePair<RegKey, T>> GetEnumerator() => _regDict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}