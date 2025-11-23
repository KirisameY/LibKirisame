using KirisameLib.Data.Registers;

namespace KirisameLib.Data.Registering;

/// <summary>
///     Interface for registering item into a <see cref="IRegTarget{TKey, TItem}"/>.
/// </summary>
/// <seealso cref="RegisterBuilder{TKey, TItem}"/>
public interface IRegistrant<out TKey, out TItem> where TKey : notnull
{
    void AcceptTarget(IRegTarget<TKey, TItem> target);
}