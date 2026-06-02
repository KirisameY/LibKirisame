using JetBrains.Annotations;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registry;

namespace KirisameY.Registration.Registering;

/// <summary>
///     Builder for create an immutable <see cref="IRegistry"/>, which implemented by <see cref="FrozenRegistry{T}"/> <br/>
///     Work with <see cref="IRegistrant{T}"/> and <see cref="IRegisterDoneEventSource"/>, will freeze inner <see cref="MutableRegistry{T}"/>
///     when the event source raised RegisterDone event and allow external access through the returned proxy object.
/// </summary>
/// <remarks> Both a fallback and an event source is necessary to build a register. </remarks>
[PublicAPI]
public class RegistryBuilder<T>
{
    private Func<RegKey, T>? _fallback;
    private IRegisterDoneEventSource? _eventSource;

    private readonly HashSet<IRegistrant<T>> _registrants = [];

    /// <summary>
    ///     Set a default item as fallback.<br/>
    ///     One of overloads of this method is necessary to build a register.
    /// </summary>
    public RegistryBuilder<T> WithFallback(T fallback) => WithFallback(_ => fallback);

    /// <summary>
    ///     Set a fallback function for items that are not registered.<br/>
    ///     One of overloads of this method is necessary to build a register.
    /// </summary>
    public RegistryBuilder<T> WithFallback(Func<RegKey, T> fallback)
    {
        _fallback = fallback;
        return this;
    }

    /// <summary>
    ///     Set register done event source, after the source raised RegisterDone event, the created register will be frozen and available.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public RegistryBuilder<T> WithRegisterDoneEventSource(IRegisterDoneEventSource source)
    {
        _eventSource = source;
        return this;
    }

    /// <summary>
    ///     Add a registrant for registering.
    /// </summary>
    public RegistryBuilder<T> AddRegistrant(IRegistrant<T> registrant)
    {
        _registrants.Add(registrant);
        return this;
    }

    /// <summary>
    ///     Build a register with current settings.
    /// </summary>
    /// <returns>The created register, will available after the event source raised RegisterDone event.</returns>
    /// <exception cref="InvalidOperationException"> Fallback or RegisterDoneEventSource is not set. </exception>
    public IEnumerableRegistry<T> Build()
    {
        if (_fallback is null) throw new InvalidOperationException("Fallback is not set.");
        if (_eventSource is null) throw new InvalidOperationException("RegisterDoneEventSource is not set.");

        var result = new PreFrozenRegisterProxy<T>();
        var mutable = new MutableRegistry<T>(_fallback);

        result.InnerRegister      =  mutable;
        _eventSource.RegisterDone += () => result.InnerRegister = mutable.Freeze();

        foreach (var registrant in _registrants)
        {
            registrant.AcceptTarget(mutable);
        }

        return result;
    }
}