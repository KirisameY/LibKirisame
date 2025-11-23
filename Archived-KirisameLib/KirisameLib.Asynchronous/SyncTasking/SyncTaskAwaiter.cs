using System.Runtime.CompilerServices;

namespace KirisameLib.Asynchronous.SyncTasking;

public readonly struct SyncTaskAwaiter(SyncTask task, bool restoreContext = true) : ICriticalNotifyCompletion
{
    public bool IsCompleted => task.IsCompleted;

    public void GetResult()
    {
        if (task.IsCompletedSuccessfully) return;
        if (task.IsCanceled) throw new SyncTaskCancelledException(task, task.CancellationToken!.Value);
        if (task.IsFaulted)
        {
            if (task.Exception!.InnerExceptions is [{ } e]) throw e;
            throw task.Exception;
        }
        throw new InvalidOperationException("Unable to get result of an uncompleted SyncTask");
    }

    public void OnCompleted(Action continuation)
    {
        var capturedContext = ExecutionContext.Capture();

        Action wrappedContinuation = () =>
        {
            if (capturedContext != null)
            {
                ExecutionContext.Run(capturedContext, state => ((Action)state!)(), continuation);
            }
            else
            {
                continuation();
            }
        };

        task.ContinueWith(wrappedContinuation, restoreContext);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        task.ContinueWith(continuation, restoreContext);
    }
}

public readonly struct SyncTaskAwaiter<T>(SyncTask<T> task, bool restoreContext = true) : ICriticalNotifyCompletion
{
    public bool IsCompleted => task.IsCompleted;

    public T GetResult()
    {
        if (task.IsCompletedSuccessfully) return task.Result;
        if (task.IsCanceled) throw new SyncTaskCancelledException(task, task.CancellationToken!.Value);
        if (task.IsFaulted)
        {
            if (task.Exception!.InnerExceptions is [{ } e]) throw e;
            throw task.Exception;
        }
        throw new InvalidOperationException("Unable to get result of an uncompleted SyncTask");
    }


    public void OnCompleted(Action continuation)
    {
        var capturedContext = ExecutionContext.Capture();

        Action wrappedContinuation = () =>
        {
            if (capturedContext != null)
            {
                ExecutionContext.Run(capturedContext, state => ((Action)state!)(), continuation);
            }
            else
            {
                continuation();
            }
        };

        task.ContinueWith(wrappedContinuation, restoreContext);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        task.ContinueWith(continuation, restoreContext);
    }
}