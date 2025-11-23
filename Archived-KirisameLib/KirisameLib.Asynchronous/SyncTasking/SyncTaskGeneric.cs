using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace KirisameLib.Asynchronous.SyncTasking;

[AsyncMethodBuilder(typeof(SyncTaskBuilder<>))]
public class SyncTask<T> : SyncTask
{
    internal SyncTask() { }


    /// <summary>
    ///     The result value of this SyncTask, which is of the same type as the task's type parameter.<br/>
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when this SyncTask is not completed.</exception>
    /// <exception cref="AggregateException">
    ///     The task was canceled. The InnerExceptions collection contains a TaskCanceledException object.
    ///     <para>-or-</para>
    ///     An exception was thrown during the execution of the task. The InnerExceptions collection contains information about the exception or exceptions.
    /// </exception>
    [field: AllowNull, MaybeNull]
    public T Result
    {
        get => Status switch
        {
            SyncTaskStatus.RanToCompletion => field!,
            SyncTaskStatus.Canceled        => throw new AggregateException(new SyncTaskCancelledException(this, CancellationToken!.Value)),
            SyncTaskStatus.Faulted         => throw new AggregateException(Exception!),

            _ => throw new InvalidOperationException("Unable to get result of an uncompleted SyncTask")
        };
        private set;
    }


    internal void SetResult(T result)
    {
        lock (Lock)
        {
            if (Status is not SyncTaskStatus.Running) return;
            Status = SyncTaskStatus.RanToCompletion;
            Result = result;
        }
        Continuation?.Invoke();
    }

    public new SyncTaskAwaiter<T> GetAwaiter() => new(this);

    public new ConfiguredSyncTaskAwaitable<T> ConfigureAwait(bool restoreContext) => new(this, restoreContext);


    public SyncTask ContinueWith(Action<SyncTask<T>> continuation, bool restoreContext = true)
    {
        SyncTask result = new();
        var ctx = SynchronizationContext.Current;
        var action = () =>
        {
            if (!restoreContext || SynchronizationContext.Current == ctx || ctx is null) RunContinue(this, continuation, result);
            else ctx.Post(_ => RunContinue(this, continuation, result), null);
        };

        var completed = false;
        lock (Lock)
        {
            if (IsCompleted) completed =  true;
            else Continuation          += action;
        }
        if (completed) action.Invoke();

        return result;

        static void RunContinue(SyncTask<T> task, Action<SyncTask<T>> continuation, SyncTask newTask)
        {
            newTask.Start();

            try { continuation.Invoke(task); }
            catch (OperationCanceledException e)
            {
                newTask.SetCanceled(e.CancellationToken);
                return;
            }
            catch (Exception e)
            {
                newTask.SetFailed(e);
                return;
            }

            newTask.SetResult();
        }
    }

    public SyncTask<TResult> ContinueWith<TResult>(Func<SyncTask<T>, TResult> continuation, bool restoreContext = true)
    {
        SyncTask<TResult> result = new();
        var ctx = SynchronizationContext.Current;
        var action = () =>
        {
            if (!restoreContext || SynchronizationContext.Current == ctx || ctx is null) RunContinue(this, continuation, result);
            else ctx.Post(_ => RunContinue(this, continuation, result), null);
        };

        var completed = false;
        lock (Lock)
        {
            if (IsCompleted) completed =  true;
            else Continuation          += action;
        }
        if (completed) action.Invoke();

        return result;

        static void RunContinue(SyncTask<T> task, Func<SyncTask<T>, TResult> continuation, SyncTask<TResult> newTask)
        {
            newTask.Start();
            TResult result;

            try { result = continuation.Invoke(task); }
            catch (OperationCanceledException e)
            {
                newTask.SetCanceled(e.CancellationToken);
                return;
            }
            catch (Exception e)
            {
                newTask.SetFailed(e);
                return;
            }

            newTask.SetResult(result);
        }
    }
}