using System.Runtime.CompilerServices;

namespace KirisameLib.Asynchronous;

public readonly struct SwitchToThreadPoolAwaitable
{
    public SwitchToThreadPoolAwaiter GetAwaiter() => new SwitchToThreadPoolAwaiter();
}

public readonly struct SwitchToThreadPoolAwaiter : INotifyCompletion
{
    public bool IsCompleted => false;
    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        ThreadPool.QueueUserWorkItem(_ => continuation());
    }
}