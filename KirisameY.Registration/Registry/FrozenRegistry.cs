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

    public T GetItem(RegKey key, out bool succeed)
    {
        succeed = _regDict.TryGetValue(key, out var value);
        if (!succeed)
        {
            try { value = fallback(key); }
            catch (Exception e)
            {
                throw new GettingFallbackValueFailedException(this, key, e);
            }
        }

        return value!;
    }

    public bool ItemRegistered(RegKey key) => _regDict.ContainsKey(key);


    public IEnumerator<KeyValuePair<RegKey, T>> GetEnumerator() => _regDict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool TryGetValue(RegKey key, out T value)
    {
        value = GetItem(key, out var succeed);
        return succeed;
    }

    public int Count => _regDict.Count;

    public T this[RegKey key] => GetItem(key, out _);
    public IEnumerable<RegKey> Keys => _regDict.Keys;
    public IEnumerable<T> Values => _regDict.Values;
}