using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using KirisameY.SyncOrder.Await;

namespace KirisameY.SyncOrder.Async;

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public readonly struct OrderMethodBuilder<T>()
{
    [UsedImplicitly]
    public static OrderMethodBuilder<T> Create() => new();


    private class OrderContainer
    {
        public Order<T>? Order;
        public OrderCompletionSource<T>? CompletionSource;
    }

    private readonly OrderContainer _orderContainer = new();


    [UsedImplicitly, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        IAsyncStateMachine boxed = stateMachine;

        var completionSource = _orderContainer.CompletionSource = new();
        _orderContainer.Order = new(boxed.MoveNext, completionSource.Token);
    }

    [UsedImplicitly]
    public Order<T> Task => _orderContainer.Order!;

    [UsedImplicitly]
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }


    [UsedImplicitly]
    public void SetResult(T result)
    {
        _orderContainer.CompletionSource!.Complete(result);
    }

    [UsedImplicitly]
    public void SetException(Exception exception)
    {
        throw new OrderAsyncMethodRunningException(exception);
    }

    [UsedImplicitly]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is not IOrderAwaiter orderAwaiter) throw new OrderAsyncMethodAwaitOtherAwaitableException(awaiter.GetType());
        var order = orderAwaiter.Order;

        if (order.IsCompleted) stateMachine.MoveNext();
        else
        {
            IAsyncStateMachine boxed = stateMachine;
            order.ContinueWith(boxed.MoveNext).Submit();
        }
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        throw new OrderAsyncMethodAwaitOtherAwaitableException(awaiter.GetType());
    }
}