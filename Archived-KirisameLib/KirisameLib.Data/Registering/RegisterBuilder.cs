using JetBrains.Annotations;

using KirisameLib.Data.Registers;

namespace KirisameLib.Data.Registering;

/// <summary>
///     Builder for create an immutable <see cref="IRegister{TKey, TItem}"/>, which implemented by <see cref="FrozenRegister{TKey, TItem}"/> <br/>
///     Work with <see cref="IRegistrant{TKey, TItem}"/> and <see cref="IRegisterDoneEventSource"/>, will freeze inner <see cref="MoltenRegister{TKey, TItem}"/>
///     when the event source raised RegisterDone event and allow external access through the returned proxy object.
/// </summary>
/// <remarks> Both a fallback and an event source is necessary to build a register. </remarks>
[PublicAPI]
public class RegisterBuilder<TKey, TItem> where TKey : notnull
{
    private Func<TKey, TItem>? _fallback;
    private IRegisterDoneEventSource? _eventSource;

    private readonly HashSet<IRegistrant<TKey, TItem>> _registrants = [];

    /// <summary>
    ///     Set a default item as fallback.<br/>
    ///     One of overloads of this method is necessary to build a register.
    /// </summary>
    public RegisterBuilder<TKey, TItem> WithFallback(TItem fallback) => WithFallback(_ => fallback);

    /// <summary>
    ///     Set a fallback function for items that are not registered.<br/>
    ///     One of overloads of this method is necessary to build a register.
    /// </summary>
    public RegisterBuilder<TKey, TItem> WithFallback(Func<TKey, TItem> fallback)
    {
        _fallback = fallback;
        return this;
    }

    /// <summary>
    ///     Set register done event source, after the source raised RegisterDone event, the created register will be frozen and available.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public RegisterBuilder<TKey, TItem> WithRegisterDoneEventSource(IRegisterDoneEventSource source)
    {
        _eventSource = source;
        return this;
    }

    /// <summary>
    ///     Add a registrant for registering.
    /// </summary>
    public RegisterBuilder<TKey, TItem> AddRegistrant(IRegistrant<TKey, TItem> registrant)
    {
        _registrants.Add(registrant);
        return this;
    }

    /// <summary>
    ///     Build a register with current settings.
    /// </summary>
    /// <returns>The created register, will available after the event source raised RegisterDone event.</returns>
    /// <exception cref="InvalidOperationException"> Fallback or RegisterDoneEventSource is not set. </exception>
    public IEnumerableRegister<TKey, TItem> Build()
    {
        if (_fallback is null) throw new InvalidOperationException("Fallback is not set.");
        if (_eventSource is null) throw new InvalidOperationException("RegisterDoneEventSource is not set.");

        var result = new PreFrozenProxyRegister<TKey, TItem>();
        var molten = new MoltenRegister<TKey, TItem>(_fallback);

        _eventSource.RegisterDone += () => result.InnerRegister = molten.Freeze();
        foreach (var registrant in _registrants)
        {
            registrant.AcceptTarget(molten);
        }

        return result;
    }
}