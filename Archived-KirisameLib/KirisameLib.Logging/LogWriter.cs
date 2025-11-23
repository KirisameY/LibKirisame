using System.Diagnostics;

using KirisameLib.Asynchronous;
using KirisameLib.Extensions;

namespace KirisameLib.Logging;

public sealed class LogWriter : IDisposable
{
    public LogWriter(params TextWriter[] logWriters)
    {
        _logWriters = logWriters;
        _writingTask = StartWriteLogAsync(_cancellationTokenSource.Token);
    }

    private readonly TextWriter[] _logWriters;
    private readonly Queue<Log> _logQueue = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _writingTask;

    public int CycleInterval { get; set; } = 1000;

    public void Log(Log log) => _logQueue.Enqueue(log);

    private async Task StartWriteLogAsync(CancellationToken cancellationToken)
    {
        await AsyncOrrery.SwitchContext();

        while (!cancellationToken.IsCancellationRequested)
        {
            try { await Task.Delay(CycleInterval, cancellationToken); }
            catch (TaskCanceledException) { }

            while (_logQueue.TryDequeue(out var log))
            {
                foreach (var writer in _logWriters)
                {
                    writer.WriteLine(log.ToString());
                    writer.Flush();
                }
            }
        }
        _logWriters.ForEach(writer => writer.Dispose());
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _writingTask.Wait();
        _cancellationTokenSource.Dispose();
    }
}