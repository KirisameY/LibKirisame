using JetBrains.Annotations;

namespace KirisameY.EventBus.EventCompoments;

public struct EventCancelToken()
{
    public bool Canceled { get; private set; } = false;

    [PublicAPI]
    public bool Cancel()
    {
        (var alreadyCanceled, Canceled) = (Canceled, true);
        return alreadyCanceled;
    }
}