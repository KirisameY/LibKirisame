using System.Collections;
using System.Diagnostics.CodeAnalysis;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registry;

namespace KirisameY.Registration.Registering;

/// <summary>
///     Internal class, used for <see cref="RegistryBuilder{T}"/>.
/// </summary>
internal class PreFrozenRegisterProxy<T> : IEnumerableRegistry<T>
{
    [field: AllowNull, MaybeNull]
    internal IEnumerableRegistry<T> InnerRegister
    {
        get => field ?? throw new RegisterNotInitializedException();
        set;
    }

    public T this[RegKey key] => InnerRegister[key];
    public IEnumerable<RegKey> Keys => InnerRegister.Keys;
    public IEnumerable<T> Values => InnerRegister.Values;
    public int Count => InnerRegister.Count;

    public T GetItem(RegKey id) => InnerRegister.GetItem(id);
    public bool ItemRegistered(RegKey id) => InnerRegister.ItemRegistered(id);
    public bool TryGetValue(RegKey key, [MaybeNullWhen(false)] out T value) => InnerRegister.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<RegKey, T>> GetEnumerator() => InnerRegister.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)InnerRegister).GetEnumerator();
}

public class RegisterNotInitializedException() : InvalidOperationException("Try to visit register proxy before it initialized");