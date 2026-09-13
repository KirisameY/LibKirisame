using KirisameY.SyncOrder.Await;

namespace KirisameY.SyncOrder.Test.OrderTests;

using static OrderTestUtils;

public class ExceptionTests
{
    [Fact]
    public void OrderConsumed()
    {
        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder();
            order.Submit();
            order.Submit();
        });

        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder();
            order.Submit();
            order.ContinueWith(() => { });
        });

        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder();
            order.ContinueWith(() => { });
            order.Submit();
        });

        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder();
            order.ContinueWith(() => { });
            order.ContinueWith(() => { });
        });

        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder(1);
            order.Submit();
            order.Submit();
        });

        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder(1);
            order.Submit();
            order.ContinueWith(() => { });
        });

        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder(1);
            order.ContinueWith(() => { });
            order.Submit();
        });

        Assert.Throws<OrderConsumedException>(() =>
        {
            var order = GetOrder(1);
            order.ContinueWith(() => { });
            order.ContinueWith(() => { });
        });
    }

    [Fact]
    public void OrderUnsubmitted()
    {
        Assert.Throws<OrderUnsubmittedException>(() =>
        {
            OrderCompletionSource completionSource = new();
            Order _ = new(() => { }, completionSource.Token);
            completionSource.Complete();
        });

        Assert.Throws<OrderUnsubmittedException>(() =>
        {
            OrderCompletionSource<int> completionSource = new();
            Order<int> _ = new(() => { }, completionSource.Token);
            completionSource.Complete(0);
        });
    }

    [Fact]
    public void OrderUncompleted()
    {
        Assert.Throws<OrderUncompletedException>(() =>
        {
            OrderCompletionSource<int> completionSource = new();
            Order<int> order = new(() => { }, completionSource.Token);
            _ = order.Result;
        });
    }

    [Fact]
    public void OrderSourceAlreadyCompleted()
    {
        Assert.Throws<OrderSourceAlreadyCompletedException>(() =>
        {
            OrderCompletionSource completionSource = new();
            completionSource.Complete();
            completionSource.Complete();
        });

        Assert.Throws<OrderSourceAlreadyCompletedException>(() =>
        {
            OrderCompletionSource<int> completionSource = new();
            completionSource.Complete(0);
            completionSource.Complete(1);
        });

        Assert.Throws<OrderSourceAlreadyCompletedException>(() =>
        {
            OrderCompletionSource completionSource = new();
            completionSource.Complete();
            _ = new Order(() => { }, completionSource.Token);
        });

        Assert.Throws<OrderSourceAlreadyCompletedException>(() =>
        {
            OrderCompletionSource<int> completionSource = new();
            completionSource.Complete(0);
            _ = new Order<int>(() => { }, completionSource.Token);
        });
    }

    [Fact]
    public void ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            OrderCompletionSource completionSource = new();
            OrderCompletionSource<int> completionSourceInt = new();
            completionSource.Complete();
            completionSourceInt.Complete(0);
        });

        Assert.Null(exception);
    }
}