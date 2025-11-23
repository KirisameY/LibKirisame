using System.Collections;
using System.Diagnostics.CodeAnalysis;

using KirisameLib.Data.Registers;

namespace KirisameLib.Data.Registering;

/// <summary>
///     Internal class, used for <see cref="RegisterBuilder{TKey, TItem}"/>.
/// </summary>
internal class PreFrozenProxyRegister<TKey, TItem> : IEnumerableRegister<TKey, TItem> where TKey : notnull
{
    [field: AllowNull, MaybeNull]
    internal IEnumerableRegister<TKey, TItem> InnerRegister
    {
        get => field ?? throw new RegisterNotInitializedException();
        set
        {
            if (field is not null) throw new RegisterAlreadyInitializedException();
            field = value;
        }
    }


    public TItem this[TKey key] => InnerRegister[key];
    public IEnumerable<TKey> Keys => InnerRegister.Keys;
    public IEnumerable<TItem> Values => InnerRegister.Values;
    public int Count => InnerRegister.Count;

    public TItem GetItem(TKey id) => InnerRegister.GetItem(id);
    public bool ItemRegistered(TKey id) => InnerRegister.ItemRegistered(id);
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TItem value) => InnerRegister.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TItem>> GetEnumerator() => InnerRegister.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)InnerRegister).GetEnumerator();
}

public class RegisterNotInitializedException() : InvalidOperationException("Try to visit register before registration done");

public class RegisterAlreadyInitializedException() : InvalidOperationException("Register already initialized");