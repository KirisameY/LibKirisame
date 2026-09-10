using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using KirisameY.SyncOrder.Async;

namespace KirisameY.SyncOrder;

[AsyncMethodBuilder(typeof(OrderMethodBuilder))]
public class Order
{
    public Order(Func<bool> submit, out Action complete)
    {
        _submit  = submit;
        complete = Complete;
    }

    private protected bool _consumed = false;

    public bool IsCompleted { get; private set; } = false;


    private Func<bool>? _submit;

    [PublicAPI]
    public void Submit()
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        DoSubmit();
    }

    private protected bool DoSubmit()
    {
        if (_submit is null) throw new OrderDuplicateSubmitException(this);

        var completed = _submit.Invoke();
        _submit = null;
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
        if (_submit is not null) throw new OrderUnsubmittedException(this);
        if (IsCompleted) throw new OrderDuplicateCompleteException(this);
        IsCompleted = true;

        _continuation?.Invoke();
    }
}

[AsyncMethodBuilder(typeof(OrderMethodBuilder<>))]
public sealed class Order<T> : Order // 等待C#15的union
{
    public Order(Func<bool> submit, out Action<T> complete) : base(submit, out var c)
    {
        complete = result =>
        {
            _result = result;
            c.Invoke();
        };
    }

    private T? _result = default;
    public T Result => IsCompleted ? _result! : throw new OrderUncompletedException(this);


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