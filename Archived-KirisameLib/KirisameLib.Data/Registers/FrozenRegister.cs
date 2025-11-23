using System.Collections;
using System.Collections.Frozen;

using KirisameLib.Data.Registering;

namespace KirisameLib.Data.Registers;

/// <summary>
///     An immutable register.<br/>
///     The most common implementation of <see cref="IRegister{TKey, TItem}"/>.
/// </summary>
/// <param name="regDict">
///     Source dictionary that contains all registered id-item pairs.<br/>
///     Will not be stored in the new instance.
/// </param>
/// <param name="fallback"> Fallback function for items that are not registered. </param>
/// <seealso cref="MoltenRegister{TKey, TItem}"/>
/// <seealso cref="RegisterBuilder{TKey, TItem}"/>
public class FrozenRegister<TKey, TItem>(IDictionary<TKey, TItem> regDict, Func<TKey, TItem> fallback) : IEnumerableRegister<TKey, TItem> where TKey : notnull
{
    private readonly FrozenDictionary<TKey, TItem> _regDict = regDict.ToFrozenDictionary();

    public TItem this[TKey id] => GetItem(id);
    public IEnumerable<TKey> Keys => _regDict.Keys;
    public IEnumerable<TItem> Values => _regDict.Values;
    public int Count => _regDict.Count;

    public TItem GetItem(TKey id)
    {
        if (!_regDict.TryGetValue(id, out var value))
        {
            try { value = fallback(id); }
            catch (Exception e)
            {
                throw new GettingFallbackValueFailedException(id, e);
            }
        }

        return value;
    }

    public bool ItemRegistered(TKey id) => _regDict.ContainsKey(id);

    public bool TryGetValue(TKey key, out TItem value)
    {
        bool result = _regDict.TryGetValue(key, out var item);
        value = result ? item! : fallback(key);
        return result;
    }

    public IEnumerator<KeyValuePair<TKey, TItem>> GetEnumerator() => _regDict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}