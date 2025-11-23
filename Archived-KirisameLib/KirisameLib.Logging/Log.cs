namespace KirisameLib.Logging;

public readonly record struct Log(LogLevel Level, string Source, string Process, string Message)
{
    private DateTime Time { get; } = DateTime.Now;

    public override string ToString() =>
        $"{Time:yyyy-MM-dd HH:mm:ss.fff} |{Level}| [{Source}] [{Process}]: {Message}";
}