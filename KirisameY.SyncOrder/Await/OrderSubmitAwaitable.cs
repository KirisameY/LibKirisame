namespace KirisameY.SyncOrder.Await;

public readonly struct OrderSubmitAwaitable(Order order)
{
    public OrderAwaiter GetAwaiter() => new(order, true);
}

public readonly struct OrderSubmitAwaitable<T>(Order<T> order)
{
    public OrderAwaiter<T> GetAwaiter() => new(order, true);
}