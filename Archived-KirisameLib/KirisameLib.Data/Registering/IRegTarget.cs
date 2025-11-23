namespace KirisameLib.Data.Registering;

public interface IRegTarget<in TKey, in TItem> where TKey : notnull
{
    /// <summary>
    ///     Whether the register target is available to register.
    /// </summary>
    bool AvailableToReg { get; }

    /// <summary>
    ///     Try to add an item to the register.
    /// </summary>
    /// <returns> Whether the item is added successfully. </returns>
    /// <exception cref="RegisterTargetUnavailableException"> Register target is unavailable. </exception>
    bool AddItem(TKey id, TItem item);

    /// <summary>
    ///     Add an item to the register, if already exists, overwrite it.
    /// </summary>
    /// <exception cref="RegisterTargetUnavailableException"> Register target is unavailable. </exception>
    public void AddOrOverwriteItem(TKey id, TItem item);
}

public class RegisterTargetUnavailableException() : InvalidOperationException("Register target is unavailable");