using System.Collections.Immutable;

namespace KirisameLib.Asynchronous.SyncTasking;

public partial class SyncTask
{
    #region Completed

    public static SyncTask Completed()
    {
        var result = new SyncTask();
        result.Start();
        result.SetResult();
        return result;
    }

    public static SyncTask<T> FromResult<T>(T result)
    {
        var task = new SyncTask<T>();
        task.Start();
        task.SetResult(result);
        return task;
    }

    public static SyncTask FromCancellation(CancellationToken token)
    {
        var task = new SyncTask();
        task.SetCanceled(token);
        return task;
    }

    public static SyncTask<T> FromCancellation<T>(CancellationToken token)
    {
        var task = new SyncTask<T>();
        task.SetCanceled(token);
        return task;
    }

    public static SyncTask FromException(Exception exception)
    {
        var task = new SyncTask();
        task.Start();
        task.SetFailed(exception);
        return task;
    }

    public static SyncTask<T> FromException<T>(Exception exception)
    {
        var task = new SyncTask<T>();
        task.Start();
        task.SetFailed(exception);
        return task;
    }

    #endregion

    #region Combine

    public static SyncTask WhenAll(ICollection<SyncTask> tasks)
    {
        if (tasks.Count == 0) return Completed();

        var result = new SyncTask();
        result.Start();

        int i = tasks.Count;
        bool cancelled = false;
        bool faulted = false;
        List<Exception> exceptions = [];
        Lock @lock = new();

        foreach (var task in tasks)
        {
            task.ContinueWith(t =>
            {
                bool complete = false;
                // 上锁判断状态以避免竞态
                lock (@lock)
                {
                    if (t.IsCanceled)
                    {
                        cancelled = true;
                        exceptions.Add(new SyncTaskCancelledException(t, t.CancellationToken!.Value));
                    }
                    else if (t.IsFaulted)
                    {
                        faulted = true;
                        exceptions.AddRange(t.Exception!.InnerExceptions);
                    }

                    i--;
                    if (i == 0) complete = true;
                }

                if (!complete) return;
                if (faulted) result.SetFailed(new AggregateException(exceptions));
                else if (cancelled) result.SetCanceled(System.Threading.CancellationToken.None);
                else result.SetResult();
            }, false);
        }

        return result;
    }

    public static SyncTask<ImmutableArray<T>> WhenAll<T>(ICollection<SyncTask<T>> tasks)
    {
        if (tasks.Count == 0) return FromResult(new ImmutableArray<T>());

        var result = new SyncTask<ImmutableArray<T>>();
        result.Start();

        int i = tasks.Count;
        T[] resultArray = new T[i];
        bool cancelled = false;
        bool faulted = false;
        List<Exception> exceptions = [];
        Lock @lock = new();

        foreach (var (index, task) in tasks.Index())
        {
            task.ContinueWith(t =>
            {
                bool complete = false;
                // 上锁判断状态以避免竞态
                lock (@lock)
                {
                    if (t.IsCanceled)
                    {
                        cancelled = true;
                        exceptions.Add(new SyncTaskCancelledException(t, t.CancellationToken!.Value));
                    }
                    else if (t.IsFaulted)
                    {
                        faulted = true;
                        exceptions.AddRange(t.Exception!.InnerExceptions);
                    }
                    else resultArray[index] = t.Result;

                    i--;
                    if (i == 0) complete = true;
                }

                if (!complete) return;
                if (faulted) result.SetFailed(new AggregateException(exceptions));
                else if (cancelled) result.SetCanceled(System.Threading.CancellationToken.None);
                else result.SetResult(resultArray.ToImmutableArray());
            }, false);
        }

        return result;
    }


    public static SyncTask<SyncTask> WhenAny(ICollection<SyncTask> tasks)
    {
        if (tasks.Count == 0)
        {
            // 遵循 Task.WhenAny 的规范，抛出异常
            throw new ArgumentException("The tasks collection must not be null or empty.", nameof(tasks));
        }

        var result = new SyncTask<SyncTask>();
        result.Start();

        // 0 表示未完成，1 表示已完成
        int completedState = 0;

        Action<SyncTask> continuation = completedTask =>
        {
            // 原子性尝试赋值并返回原值，若返回0则说明先前尚未有任务完成
            if (Interlocked.CompareExchange(ref completedState, 1, 0) == 0)
            {
                result.SetResult(completedTask);
            }
        };

        foreach (var task in tasks)
        {
            if (task.IsCompleted)
            {
                // 如果已有任务完成了，立即触发 continuation
                continuation(task);
                break;
            }
            task.ContinueWith(continuation, false);
        }

        return result;
    }

    public static SyncTask<SyncTask<T>> WhenAny<T>(ICollection<SyncTask<T>> tasks)
    {
        if (tasks.Count == 0)
        {
            // 遵循 Task.WhenAny 的规范，抛出异常
            throw new ArgumentException("The tasks collection must not be null or empty.", nameof(tasks));
        }

        var result = new SyncTask<SyncTask<T>>();
        result.Start();

        // 0 表示未完成，1 表示已完成
        int completedState = 0;

        Action<SyncTask<T>> continuation = completedTask =>
        {
            // 原子性尝试赋值并返回原值，若返回0则说明先前尚未有任务完成
            if (Interlocked.CompareExchange(ref completedState, 1, 0) == 0)
            {
                result.SetResult(completedTask);
            }
        };

        foreach (var task in tasks)
        {
            if (task.IsCompleted)
            {
                // 如果已有任务完成了，立即触发 continuation
                continuation(task);
                break;
            }
            task.ContinueWith(continuation, false);
        }

        return result;
    }

    #endregion
}