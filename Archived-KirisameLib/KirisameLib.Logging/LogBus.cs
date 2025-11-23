namespace KirisameLib.Logging;

public abstract class LogBus(LogLevel minLogLevel) : IDisposable
{
    public LogLevel MinLogLevel { get; } = minLogLevel;

    public Logger GetLogger(string source) => new Logger(this, source);

    public abstract void Log(Log log);

    protected virtual void Disposing() { }

    public void Dispose()
    {
        Disposing();
        GC.SuppressFinalize(this);
    }
}