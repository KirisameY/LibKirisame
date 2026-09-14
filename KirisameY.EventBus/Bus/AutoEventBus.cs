namespace KirisameY.EventBus.Bus;

public class AutoEventBus : EventBusBase
{
    protected override void PostEnqueued() => HandleQueue();
}