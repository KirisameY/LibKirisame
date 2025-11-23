namespace KirisameLib.Asynchronous.SyncTasking;

public class SyncTaskCompletionSource
{
    public SyncTask Task { get; } = new();


    public bool TrySetResult()
    {
        if (Task.IsCompleted) return false;
        Task.SetResult();
        return true;
    }

    public void SetResult()
    {
        if (Task.IsCompleted) throw new InvalidOperationException("Task has already completed");
        Task.SetResult();
    }

    public bool TrySetCanceled() => TrySetCanceled(CancellationToken.None);

    public bool TrySetCanceled(CancellationToken token)
    {
        if (Task.IsCompleted) return false;
        Task.SetCanceled(token);
        return true;
    }

    public void SetCanceled() => SetCanceled(CancellationToken.None);

    public void SetCanceled(CancellationToken token)
    {
        if (Task.IsCompleted) throw new InvalidOperationException("Task has already completed.");
        Task.SetCanceled(token);
    }

    public bool TrySetException(params IEnumerable<Exception> exceptions) => TrySetException(new AggregateException(exceptions));

    public bool TrySetException(Exception exception)
    {
        if (Task.IsCompleted) return false;
        Task.SetFailed(exception);
        return true;
    }

    public void SetException(params IEnumerable<Exception> exceptions) => SetException(new AggregateException(exceptions));

    public void SetException(Exception exception)
    {
        if (Task.IsCompleted) throw new InvalidOperationException("Task has already completed.");
        Task.SetFailed(exception);
    }
}


public class SyncTaskCompletionSource<T>
{
    public SyncTask<T> Task { get; } = new();


    public bool TrySetResult(T result)
    {
        if (Task.IsCompleted) return false;
        Task.SetResult(result);
        return true;
    }

    public void SetResult(T result)
    {
        if (Task.IsCompleted) throw new InvalidOperationException("Task has already completed");
        Task.SetResult(result);
    }

    public bool TrySetCanceled() => TrySetCanceled(CancellationToken.None);

    public bool TrySetCanceled(CancellationToken token)
    {
        if (Task.IsCompleted) return false;
        Task.SetCanceled(token);
        return true;
    }

    public void SetCanceled() => SetCanceled(CancellationToken.None);

    public void SetCanceled(CancellationToken token)
    {
        if (Task.IsCompleted) throw new InvalidOperationException("Task has already completed.");
        Task.SetCanceled(token);
    }

    public bool TrySetException(params IEnumerable<Exception> exceptions) => TrySetException(new AggregateException(exceptions));

    public bool TrySetException(Exception exception)
    {
        if (Task.IsCompleted) return false;
        Task.SetFailed(exception);
        return true;
    }

    public void SetException(params IEnumerable<Exception> exceptions) => SetException(new AggregateException(exceptions));

    public void SetException(Exception exception)
    {
        if (Task.IsCompleted) throw new InvalidOperationException("Task has already completed.");
        Task.SetFailed(exception);
    }
}