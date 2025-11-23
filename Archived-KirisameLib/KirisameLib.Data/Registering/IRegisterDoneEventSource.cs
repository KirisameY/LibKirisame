namespace KirisameLib.Data.Registering;

/// <summary>
///     Interface for the register done event <br/>
///     It's recommended to clear the event after raised it.
/// </summary>
/// <seealso cref="RegisterBuilder{TKey, TItem}"/>
public interface IRegisterDoneEventSource
{
    event Action RegisterDone;
}