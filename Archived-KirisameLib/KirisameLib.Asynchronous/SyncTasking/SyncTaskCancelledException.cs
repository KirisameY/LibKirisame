namespace KirisameLib.Asynchronous.SyncTasking;

[Serializable]
public class SyncTaskCancelledException(SyncTask task, CancellationToken token) : OperationCanceledException(token)
{
    public SyncTask Task => task;
}