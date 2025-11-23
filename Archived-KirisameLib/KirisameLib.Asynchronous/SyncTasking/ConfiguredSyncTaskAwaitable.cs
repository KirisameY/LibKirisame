namespace KirisameLib.Asynchronous.SyncTasking;

public class ConfiguredSyncTaskAwaitable(SyncTask task, bool restoreContext)
{
    public SyncTaskAwaiter GetAwaiter() => new(task, restoreContext);
}

public class ConfiguredSyncTaskAwaitable<T>(SyncTask<T> task, bool restoreContext)
{
    public SyncTaskAwaiter<T> GetAwaiter() => new(task, restoreContext);
}