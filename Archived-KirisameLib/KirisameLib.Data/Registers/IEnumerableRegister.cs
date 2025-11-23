namespace KirisameLib.Data.Registers;

/// <summary>
///     Readonly register interface that can be iterated as a dictionary.
/// </summary>
public interface IEnumerableRegister<TKey, TItem> : IRegister<TKey, TItem>, IReadOnlyDictionary<TKey, TItem> where TKey : notnull
{
    new TItem this[TKey key] { get; }
    bool IReadOnlyDictionary<TKey, TItem>.ContainsKey(TKey key) => ItemRegistered(key);
}