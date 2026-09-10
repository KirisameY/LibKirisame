using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace KirisameY.SyncOrder.Await;

public readonly struct OrderAwaiter(Order order) : INotifyCompletion, IOrderAwaiter
{
    Order IOrderAwaiter.Order => order;

    [UsedImplicitly]
    public bool IsCompleted => order.IsCompleted;

    [UsedImplicitly]
    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        throw new OrderAwaitedNotInOrderMethodException();
    }
}

public readonly struct OrderAwaiter<T>(Order<T> order) : INotifyCompletion, IOrderAwaiter
{
    Order IOrderAwaiter.Order => order;

    [UsedImplicitly]
    public bool IsCompleted => order.IsCompleted;

    [UsedImplicitly]
    public T GetResult() => order.Result;

    public void OnCompleted(Action continuation)
    {
        throw new OrderAwaitedNotInOrderMethodException();
    }
}