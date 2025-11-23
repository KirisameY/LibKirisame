namespace KirisameLib.Asynchronous.SyncTasking;

public static class SyncTaskExtensions
{
    public static Task AsTask(this SyncTask task)
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        task.ContinueWith(t =>
        {
            if (t.IsFaulted) source.TrySetException(t.Exception!.InnerExceptions);
            else if (t.IsCanceled) source.TrySetCanceled(t.CancellationToken!.Value);
            else source.TrySetResult();
        }, false);

        return source.Task;
    }

    public static Task<T> AsTask<T>(this SyncTask<T> task)
    {
        var source = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        task.ContinueWith(t =>
        {
            if (t.IsFaulted) source.TrySetException(t.Exception!.InnerExceptions);
            else if (t.IsCanceled) source.TrySetCanceled(t.CancellationToken!.Value);
            else source.TrySetResult(t.Result);
        }, false);

        return source.Task;
    }

    /// <summary>
    /// Create an <see cref="SyncTask"/> from this task.
    /// </summary>
    public static SyncTask AsSyncTask(this Task task)
    {
        var result = new SyncTask();
        result.Start();

        task.ContinueWith(t =>
        {
            if (t.IsFaulted) result.SetFailed(t.Exception);
            else if (t.IsCanceled)
            {
                CancellationToken token = CancellationToken.None;
                try { t.GetAwaiter().GetResult(); }
                catch (OperationCanceledException e) { token = e.CancellationToken; }
                finally { result.SetCanceled(token); }
            }
            else result.SetResult();
        }, TaskContinuationOptions.ExecuteSynchronously);
        return result;
    }

    /// <summary>
    /// Create an <see cref="SyncTask{T}"/> from this task.
    /// </summary>
    public static SyncTask<T> AsSyncTask<T>(this Task<T> task)
    {
        var result = new SyncTask<T>();
        result.Start();

        task.ContinueWith(t =>
        {
            if (t.IsFaulted) result.SetFailed(t.Exception);
            else if (t.IsCanceled)
            {
                CancellationToken token = CancellationToken.None;
                try { t.GetAwaiter().GetResult(); }
                catch (OperationCanceledException e) { token = e.CancellationToken; }
                finally { result.SetCanceled(token); }
            }
            else result.SetResult(t.Result);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return result;
    }
}