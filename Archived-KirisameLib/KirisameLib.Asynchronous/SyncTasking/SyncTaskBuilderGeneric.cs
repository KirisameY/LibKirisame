using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace KirisameLib.Asynchronous.SyncTasking;

public struct SyncTaskBuilder<T>
{
    public static SyncTaskBuilder<T> Create() => new();


    [field: AllowNull, MaybeNull]
    public SyncTask<T> Task => field ??= new();

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        Task.Start();
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) { }


    public void SetException(Exception exception)
    {
        if (exception is OperationCanceledException canceled) Task.SetCanceled(canceled.CancellationToken);
        else Task.SetFailed(exception);
    }

    public void SetResult(T result)
    {
        Task.SetResult(result);
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }
}