using KirisameLib.Data.Registering;

namespace KirisameLib.Data.Registers;

/// <summary>
///     MoltenRegister, i.e. common register before freezing.<br/>
///     Allows for registration &amp; alteration of items, and can be converted to a <see cref="FrozenRegister{TKey, TItem}"/>.<br/>
///     It is recommended to use it as an intermediate object for creating a frozen registry.
/// </summary>
/// <param name="fallback"> Fallback function for items that are not registered </param>
/// <remarks>
///     Note that this register cannot be used after freezing.
/// </remarks>
/// <seealso cref="RegisterBuilder{TKey, TItem}"/>
public class MoltenRegister<TKey, TItem>(Func<TKey, TItem> fallback) : IRegTarget<TKey, TItem> where TKey : notnull
{
    private bool _frozen = false;

    private readonly Dictionary<TKey, TItem> _regDict = new();

    public bool AvailableToReg => !_frozen;

    public bool AddItem(TKey id, TItem item)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        return _regDict.TryAdd(id, item);
    }

    public void AddOrOverwriteItem(TKey id, TItem item)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        _regDict[id] = item;
    }

    public TItem GetItem(TKey id)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
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

    public bool ItemRegistered(TKey id)
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        return _regDict.ContainsKey(id);
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
    public FrozenRegister<TKey, TItem> Freeze()
    {
        if (_frozen) throw new RegisterTargetUnavailableException();
        _frozen = true;
        return new FrozenRegister<TKey, TItem>(_regDict, fallback);
    }
}