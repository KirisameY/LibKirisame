using KirisameY.Registration.Data;

namespace KirisameY.Registration.Registering;

public interface IRegisterTarget<in TItem>
{
    /// <summary>
    ///     Whether the register target is available to add new item.
    /// </summary>
    bool IsAvailableToRegister { get; }

    /// <summary>
    ///     Try to add an item to the register.
    /// </summary>
    /// <returns> Whether the item is added successfully. </returns>
    /// <exception cref="RegisterTargetUnavailableException"> Register target is unavailable. </exception>
    bool AddItem(RegKey key, TItem item);

    /// <summary>
    ///     Add an item to the register, if already exists, overwrite it.
    /// </summary>
    /// <exception cref="RegisterTargetUnavailableException"> Register target is unavailable. </exception>
    public void AddOrOverwriteItem(RegKey key, TItem item);
}

public class RegisterTargetUnavailableException() : InvalidOperationException("Register target is unavailable");