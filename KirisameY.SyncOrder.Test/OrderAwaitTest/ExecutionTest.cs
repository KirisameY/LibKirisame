using KirisameY.SyncOrder.Await;

namespace KirisameY.SyncOrder.Test.OrderAwaitTest;

public class ExecutionTest
{
    [Fact]
    public void AsyncRun()
    {
        OrderCompletionSource completionSource = new();
        OrderCompletionSource<int> completionSourceInt = new();
        OrderCompletionSource<string> completionSourceString = new();

        List<string> list = [];

        var order = ((Func<Order>)(async () =>
        {
            list.Add("- start");
            await new Order(() => { }, completionSource.Token);
            list.Add("- first done");
            var i = await new Order<int>(() => { }, completionSourceInt.Token);
            list.Add($"- second done with [{i}]");
            var s = await new Order<string>(() => { }, completionSourceString.Token);
            list.Add($"- third done with [{s}]");
        })).Invoke();

        list.Add("order get");

        order.ContinueWith(() => list.Add("- - continue"))
             .ContinueWith(() => "asd")
             .ContinueWith(s => list.Add($"- - - continued with [{s}]"))
             .Submit();
        list.Add("order submitted");

        completionSource.Complete();
        list.Add("completed first");

        completionSourceInt.Complete(1);
        list.Add("completed second");

        completionSourceString.Complete("hello");
        list.Add("completed third");

        Assert.Equal([
            "order get",
            "- start",
            "order submitted",
            "- first done",
            "completed first",
            "- second done with [1]",
            "completed second",
            "- third done with [hello]",
            "- - continue",
            "- - - continued with [asd]",
            "completed third"
        ], list);
    }

    [Fact]
    public void AsyncRunGeneric()
    {
        OrderCompletionSource completionSource = new();
        OrderCompletionSource<int> completionSourceInt = new();
        OrderCompletionSource<string> completionSourceString = new();

        List<string> list = [];

        var order = ((Func<Order<string>>)(async () =>
        {
            list.Add("- start");
            await new Order(() => { }, completionSource.Token);
            list.Add("- first done");
            var i = await new Order<int>(() => { }, completionSourceInt.Token);
            list.Add($"- second done with [{i}]");
            var s = await new Order<string>(() => { }, completionSourceString.Token);
            list.Add($"- third done with [{s}]");
            return "asd";
        })).Invoke();

        list.Add("order get");

        order.ContinueWith(s => $"{s}!")
             .ContinueWith(s => list.Add($"- - continue with [{s}]"))
             .Submit();
        list.Add("order submitted");

        completionSource.Complete();
        list.Add("completed first");

        completionSourceInt.Complete(1);
        list.Add("completed second");

        completionSourceString.Complete("hello");
        list.Add("completed third");

        Assert.Equal([
            "order get",
            "- start",
            "order submitted",
            "- first done",
            "completed first",
            "- second done with [1]",
            "completed second",
            "- third done with [hello]",
            "- - continue with [asd!]",
            "completed third"
        ], list);
    }
}