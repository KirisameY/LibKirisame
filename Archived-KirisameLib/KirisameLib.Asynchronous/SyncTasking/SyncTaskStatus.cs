namespace KirisameLib.Asynchronous.SyncTasking;

public enum SyncTaskStatus
{
    Created,
    Running,
    RanToCompletion,
    Canceled,
    Faulted,
}