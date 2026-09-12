using KirisameY.SyncOrder.Await;

namespace KirisameY.SyncOrder.Test;

public class InvokeOrderTest
{
    [Fact]
    public void SubmitAndContinue()
    {
        List<string> list = [];

        var completionSource = new OrderCompletionSource();
        var order = new Order(() =>
        {
            list.Add("submit");
            list.Add("done");
            completionSource.Complete();
        }, completionSource.Token);

        list.Add("start");

        order = order.ContinueWith(() =>
        {
            list.Add("continue");
        });

        list.Add("add continue 1");

        order.ContinueWith(() =>
        {
            list.Add("continue2");
        }).Submit();

        // list.Add("done");
        // complete.Invoke();

        list.Add("finish");

        Assert.Equal([
            "start",
            "add continue 1",
            "submit",
            "done",
            "continue",
            "continue2",
            "finish"
        ], list);
    }

    [Fact]
    public void SubmitAndContinueWithCompleteAction()
    {
        List<string> list = [];

        var completionSource = new OrderCompletionSource();
        var order = new Order(() =>
        {
            list.Add("submit");
            // list.Add("done");
        }, completionSource.Token);

        list.Add("start");

        order = order.ContinueWith(() =>
        {
            list.Add("continue");
        });

        list.Add("add continue 1");

        order.ContinueWith(() =>
        {
            list.Add("continue2");
        }).Submit();

        list.Add("done");
        completionSource.Complete();

        list.Add("finish");

        Assert.Equal([
            "start",
            "add continue 1",
            "submit",
            "done",
            "continue",
            "continue2",
            "finish"
        ], list);
    }
}