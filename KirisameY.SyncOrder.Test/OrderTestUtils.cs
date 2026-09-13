namespace KirisameY.SyncOrder.Test;

public static class OrderTestUtils
{
    public static Order GetOrder()
    {
        var completionSource = new OrderCompletionSource();
        return new(completionSource.Complete, completionSource.Token);
    }

    public static Order<T> GetOrder<T>(T value)
    {
        var completionSource = new OrderCompletionSource<T>();
        return new(() => completionSource.Complete(value), completionSource.Token);
    }
}