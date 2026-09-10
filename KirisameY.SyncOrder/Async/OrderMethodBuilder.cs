using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using KirisameY.SyncOrder.Await;

namespace KirisameY.SyncOrder.Async;

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public readonly struct OrderMethodBuilder()
{
    [UsedImplicitly]
    public static OrderMethodBuilder Create() => new();


    private class OrderContainer
    {
        public Order? Order;
        public Action? Complete;
    }

    private readonly OrderContainer _orderContainer = new();


    [UsedImplicitly, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        IAsyncStateMachine boxed = stateMachine;

        _orderContainer.Order = new(() =>
        {
            boxed.MoveNext();
            return false;
        }, out _orderContainer.Complete);
    }

    [UsedImplicitly]
    public Order Task => _orderContainer.Order!;

    [UsedImplicitly]
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }


    [UsedImplicitly]
    public void SetResult()
    {
        _orderContainer.Complete!.Invoke();
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
}