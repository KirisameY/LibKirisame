using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using KirisameY.SyncOrder.Async;

namespace KirisameY.SyncOrder;

[AsyncMethodBuilder(typeof(OrderMethodBuilder))]
public class Order
{
    public Order(Action submit, OrderCompletionToken completionToken)
    {
        _submit                   =  submit;
        completionToken.Completed += Complete;
    }

    private protected Order(Action submit, out Action complete)
    {
        _submit  = submit;
        complete = Complete;
    }

    private protected bool _consumed = false;

    public bool IsCompleted { get; private set; } = false;


    private Action? _submit;

    [PublicAPI]
    public void Submit()
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        DoSubmit();
    }

    private protected void DoSubmit()
    {
        if (_submit is null) throw new OrderDuplicateSubmitException(this);

        (_submit, var submit) = (null, _submit);
        submit.Invoke();
    }


    private protected Action? _continuation;

    [PublicAPI]
    public Order ContinueWith(Action continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var completionSource = new OrderCompletionSource();
        var order = new Order(DoSubmit, completionSource.Token);

        _continuation = () =>
        {
            continuation.Invoke();
            completionSource.Complete();
        };

        return order;
    }

    [PublicAPI]
    public Order<TResult> ContinueWith<TResult>(Func<TResult> continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var completionSource = new OrderCompletionSource<TResult>();
        var order = new Order<TResult>(DoSubmit, completionSource.Token);

        _continuation = () =>
        {
            var result = continuation.Invoke();
            completionSource.Complete(result);
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
    public Order(Action submit, OrderCompletionToken<T> completionToken) : base(submit, out var complete)
    {
        completionToken.Completed += result =>
        {
            _result = result;
            complete.Invoke();
        };
    }

    private T? _result = default;
    public T Result => IsCompleted ? _result! : throw new OrderUncompletedException(this);


    [PublicAPI]
    public Order ContinueWith(Action<T> continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var completionSource = new OrderCompletionSource();
        var order = new Order(DoSubmit, completionSource.Token);

        _continuation = () =>
        {
            continuation.Invoke(_result!);
            completionSource.Complete();
        };

        return order;
    }

    [PublicAPI]
    public Order<TResult> ContinueWith<TResult>(Func<T, TResult> continuation)
    {
        OrderConsumedException.TryConsume(this, ref _consumed);

        var completionSource = new OrderCompletionSource<TResult>();
        var order = new Order<TResult>(DoSubmit, completionSource.Token);

        _continuation = () =>
        {
            var result = continuation.Invoke(_result!);
            completionSource.Complete(result);
        };

        return order;
    }
}