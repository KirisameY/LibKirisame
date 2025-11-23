namespace KirisameLib.Event;

[Flags]
public enum HandlerSubscribeFlag
{
    None = 0,
    OnlyOnce = 1 << 0,
    // not implemented yet
    // AllowMultiple = 1 << 1,
    // Counted = (1 << 1) + (1 << 2),
}