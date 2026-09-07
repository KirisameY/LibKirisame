using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace KirisameY.SyncOrder.Await;

public readonly struct OrderAwaiter(Order order, bool autoSubmit = false) : INotifyCompletion
{
    [UsedImplicitly]
    public bool IsCompleted => order.Completed;

    [UsedImplicitly]
    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        var newOrder = order.ContinueWith(continuation);
        if (autoSubmit) newOrder.Submit();
    }
}

public readonly struct OrderAwaiter<T>(Order<T> order, bool autoSubmit = false) : INotifyCompletion
{
    [UsedImplicitly]
    public bool IsCompleted => order.Completed;

    [UsedImplicitly]
    public T GetResult() => order.Result;

    public void OnCompleted(Action continuation)
    {
        var newOrder = order.ContinueWith(continuation);
        if (autoSubmit) newOrder.Submit();
    }
}