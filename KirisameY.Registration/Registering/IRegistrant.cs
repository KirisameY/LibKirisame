namespace KirisameY.Registration.Registering;

/// <summary>
///     Interface for registering item into a <see cref="IRegisterTarget{T}"/>.
/// </summary>
/// <seealso cref="RegistryBuilder{T}"/>
public interface IRegistrant<out T>
{
    void AcceptTarget(IRegisterTarget<T> target);
}