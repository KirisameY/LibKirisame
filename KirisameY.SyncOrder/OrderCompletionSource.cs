using JetBrains.Annotations;

namespace KirisameY.SyncOrder;

public sealed class OrderCompletionSource
{
    private bool _isCompleted = false;

    private Action? _completed;
    internal event Action? Completed
    {
        add
        {
            if (_isCompleted) throw new OrderSourceAlreadyCompletedException();
            _completed += value;
        }
        remove => _completed -= value;
    }

    [PublicAPI] public OrderCompletionToken Token => new(this);

    [PublicAPI] public void Complete()
    {
        if (_isCompleted) throw new OrderSourceAlreadyCompletedException();
        _isCompleted = true;
        _completed?.Invoke();
    }
}

public sealed class OrderCompletionSource<T>
{
    private bool _isCompleted = false;

    private Action<T>? _completed;
    internal event Action<T>? Completed
    {
        add
        {
            if (_isCompleted) throw new OrderSourceAlreadyCompletedException();
            _completed += value;
        }
        remove => _completed -= value;
    }

    [PublicAPI] public OrderCompletionToken<T> Token => new(this);

    [PublicAPI] public void Complete(T result)
    {
        if (_isCompleted) throw new OrderSourceAlreadyCompletedException();
        _isCompleted = true;
        _completed?.Invoke(result);
    }
}

public readonly struct OrderCompletionToken(OrderCompletionSource source)
{
    internal event Action Completed
    {
        add => source.Completed += value;
        remove => source.Completed -= value;
    }
}

public readonly struct OrderCompletionToken<T>(OrderCompletionSource<T> source)
{
    internal event Action<T> Completed
    {
        add => source.Completed += value;
        remove => source.Completed -= value;
    }
}