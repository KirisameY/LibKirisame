namespace KirisameLib.Event;

public sealed class CancelToken
{
    public bool Canceled { get; private set; } = false;
    public void Cancel() => Canceled = true;
}