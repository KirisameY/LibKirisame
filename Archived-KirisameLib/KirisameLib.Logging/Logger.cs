namespace KirisameLib.Logging;

public readonly struct Logger(LogBus bus, string source)
{
    public void Log(LogLevel level, string process, string message) => bus.Log(new(level, source, process, message));
}