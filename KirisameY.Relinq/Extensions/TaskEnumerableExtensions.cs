using JetBrains.Annotations;

namespace KirisameY.Relinq.Extensions;

public static class TaskEnumerableExtensions
{
    extension(IEnumerable<Task> source)
    {
        [PublicAPI]
        public Task WhenAll() => Task.WhenAll(source);

        [PublicAPI]
        public Task<Task> WhenAny() => Task.WhenAny(source);
    }

    extension<T>(IEnumerable<Task<T>> source)
    {
        [PublicAPI]
        public Task<T[]> WhenAll() => Task.WhenAll(source);

        [PublicAPI]
        public Task<Task<T>> WhenAny() => Task.WhenAny(source);
    }
}