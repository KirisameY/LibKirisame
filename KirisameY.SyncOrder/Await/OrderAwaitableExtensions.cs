using JetBrains.Annotations;

namespace KirisameY.SyncOrder.Await;

public static class OrderAwaitableExtensions
{
    extension(Order order)
    {
        [PublicAPI] public OrderAwaiter GetAwaiter() => new(order);
        [PublicAPI] public OrderSubmitAwaitable SubmitForAwait() => new(order);
    }

    extension<T>(Order<T> order)
    {
        [PublicAPI] public OrderAwaiter<T> GetAwaiter() => new(order);
        [PublicAPI] public OrderSubmitAwaitable<T> SubmitForAwait() => new(order);
    }
}