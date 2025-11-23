using System.Runtime.CompilerServices;

namespace KirisameLib.Asynchronous;

public static class AsyncOrrery
{
    public static SwitchToThreadPoolAwaitable SwitchContext() => new();
}