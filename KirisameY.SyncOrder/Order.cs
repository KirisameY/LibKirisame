using JetBrains.Annotations;

namespace KirisameY.SyncOrder;

public class Order
{
    public Order(Action submit, out Action complete)
    {
        _submit  = submit;
        complete = Complete;
    }

    private protected bool _consumed = false;


    private protected readonly Action _submit;

    [PublicAPI]
    public void Submit()
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        _submit.Invoke();
    }


    private protected Action? _continuation;

    [PublicAPI]
    public Order ContinueWith<TResult>(Action continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var order = new Order(_submit, out var complete);

        _continuation = () =>
        {
            continuation.Invoke();
            complete.Invoke();
        };

        return order;
    }

    [PublicAPI]
    public Order<TResult> ContinueWith<TResult>(Func<TResult> continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var order = new Order<TResult>(_submit, out var complete);

        _continuation = () =>
        {
            var result = continuation.Invoke();
            complete.Invoke(result);
        };

        return order;
    }

    private void Complete()
    {
        _continuation?.Invoke();
    }
}

public sealed class Order<T> : Order // 等待C#15的union
{
    public Order(Action submit, out Action<T> complete) : base(submit, out var c)
    {
        complete = t =>
        {
            _result = t;
            c.Invoke();
        };
    }

    private T? _result;


    [PublicAPI]
    public Order ContinueWith<TResult>(Action<T> continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var order = new Order(_submit, out var complete);

        _continuation = () =>
        {
            continuation.Invoke(_result!);
            complete.Invoke();
        };

        return order;
    }

    [PublicAPI]
    public Order<TResult> ContinueWith<TResult>(Func<T, TResult> continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var order = new Order<TResult>(_submit, out var complete);

        _continuation = () =>
        {
            var result = continuation.Invoke(_result!);
            complete.Invoke(result);
        };

        return order;
    }
}