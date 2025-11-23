namespace KirisameLib.Logging;

public class WriterLogBus(LogLevel minLogLevel, params TextWriter[] writers) : LogBus(minLogLevel)
{
    private readonly LogWriter _writer = new(writers);

    public override void Log(Log log)
    {
        _writer.Log(log);
    }

    protected override void Disposing()
    {
        _writer.Dispose();
    }
}