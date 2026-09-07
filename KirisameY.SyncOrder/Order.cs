using JetBrains.Annotations;

namespace KirisameY.SyncOrder;

public class Order
{
    public Order(Func<bool> submit, out Action complete)
    {
        _submit  = submit;
        complete = Complete;
    }

    private protected bool _consumed = false;

    public bool Completed { get; private set; } = false;


    private protected readonly Func<bool> _submit;

    [PublicAPI]
    public void Submit()
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        DoSubmit();
    }

    private protected bool DoSubmit()
    {
        var completed = _submit.Invoke();
        if (completed) Complete();
        return false;
    }


    private protected Action? _continuation;

    [PublicAPI]
    public Order ContinueWith(Action continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var order = new Order(DoSubmit, out var complete);

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

        var order = new Order<TResult>(DoSubmit, out var complete);

        _continuation = () =>
        {
            var result = continuation.Invoke();
            complete.Invoke(result);
        };

        return order;
    }

    private void Complete()
    {
        if (Completed) throw new OrderAlreadyCompletedException(this);
        Completed = true;

        _continuation?.Invoke();
    }
}

public sealed class Order<T> : Order // 等待C#15的union
{
    public Order(Func<bool> submit, out Action<T> complete) : base(submit, out var c)
    {
        complete = t =>
        {
            _result = t;
            c.Invoke();
        };
    }

    private T? _result = default;
    public T Result => Completed ? _result! : throw new OrderUncompletedException(this);


    [PublicAPI]
    public Order ContinueWith(Action<T> continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var order = new Order(DoSubmit, out var complete);

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

        var order = new Order<TResult>(DoSubmit, out var complete);

        _continuation = () =>
        {
            var result = continuation.Invoke(_result!);
            complete.Invoke(result);
        };

        return order;
    }
}