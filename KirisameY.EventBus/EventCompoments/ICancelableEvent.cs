using JetBrains.Annotations;

namespace KirisameY.EventBus.EventCompoments;

public interface ICancelableEvent
{
    EventCancelToken CancelToken { get; }
}

public static class CancelableEventExtensions
{
    extension(ICancelableEvent e)
    {
        [PublicAPI] public bool Canceled => e.CancelToken.Canceled;
        [PublicAPI] public bool Cancel() => e.CancelToken.Cancel();
    }
}