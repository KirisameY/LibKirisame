using System.Runtime.CompilerServices;

namespace KirisameLib.Asynchronous.SyncTasking;

[AsyncMethodBuilder(typeof(SyncTaskBuilder))]
public partial class SyncTask
{
    internal SyncTask() { }

    public SyncTaskStatus Status { get; protected set; } = SyncTaskStatus.Created;
    public AggregateException? Exception { get; private set; }
    public CancellationToken? CancellationToken { get; private set; }

    public bool IsCompleted => Status is SyncTaskStatus.RanToCompletion or SyncTaskStatus.Canceled or SyncTaskStatus.Faulted;
    public bool IsCompletedSuccessfully => Status is SyncTaskStatus.RanToCompletion;
    public bool IsCanceled => Status is SyncTaskStatus.Canceled;
    public bool IsFaulted => Status is SyncTaskStatus.Faulted;

    protected Action? Continuation;

    protected readonly Lock Lock = new();


    internal void Start()
    {
        lock (Lock)
        {
            if (Status is not SyncTaskStatus.Created) return;
            Status = SyncTaskStatus.Running;
        }
    }

    internal void SetResult()
    {
        lock (Lock)
        {
            if (Status is not SyncTaskStatus.Running) return;
            Status = SyncTaskStatus.RanToCompletion;
        }
        Continuation?.Invoke();
    }

    internal void SetCanceled(CancellationToken token)
    {
        lock (Lock)
        {
            if (Status is not (SyncTaskStatus.Created or SyncTaskStatus.Running)) return;
            Status            = SyncTaskStatus.Canceled;
            CancellationToken = token;
        }
        Continuation?.Invoke();
    }

    internal void SetFailed(Exception exception)
    {
        lock (Lock)
        {
            if (Status is not SyncTaskStatus.Running) return;
            Status = SyncTaskStatus.Faulted;
            if (exception is AggregateException e)
                Exception  = e;
            else Exception = new(exception);
        }
        Continuation?.Invoke();
    }

    public SyncTaskAwaiter GetAwaiter() => new(this);

    public ConfiguredSyncTaskAwaitable ConfigureAwait(bool restoreContext) => new(this, restoreContext);


    public SyncTask ContinueWith(Action continuation, bool restoreContext = true)
    {
        SyncTask result = new();
        var ctx = SynchronizationContext.Current;
        var action = () =>
        {
            if (!restoreContext || SynchronizationContext.Current == ctx || ctx is null) RunContinue(continuation, result);
            else ctx.Post(_ => RunContinue(continuation, result), null);
        };

        var completed = false;
        lock (Lock)
        {
            if (IsCompleted) completed =  true;
            else Continuation          += action;
        }
        if (completed) action.Invoke();

        return result;

        static void RunContinue(Action continuation, SyncTask newTask)
        {
            newTask.Start();

            try { continuation.Invoke(); }
            catch (Exception e)
            {
                newTask.SetFailed(e);
                return;
            }

            newTask.SetResult();
        }
    }

    public SyncTask ContinueWith(Action<SyncTask> continuation, bool restoreContext = true)
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

        static void RunContinue(SyncTask task, Action<SyncTask> continuation, SyncTask newTask)
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

    public SyncTask<TResult> ContinueWith<TResult>(Func<SyncTask, TResult> continuation, bool restoreContext = true)
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

        static void RunContinue(SyncTask task, Func<SyncTask, TResult> continuation, SyncTask<TResult> newTask)
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