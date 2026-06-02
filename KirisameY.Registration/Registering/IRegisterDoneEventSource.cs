namespace KirisameY.Registration.Registering;

/// <summary>
///     Interface for the register done event <br/>
///     It's recommended to clear the event after raised it.
/// </summary>
/// <seealso cref="RegistryBuilder{T}"/>
public interface IRegisterDoneEventSource
{
    event Action RegisterDone;
}