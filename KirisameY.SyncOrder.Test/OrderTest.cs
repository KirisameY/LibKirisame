using KirisameY.SyncOrder.Await;

namespace KirisameY.SyncOrder.Test;

public class OrderTest
{
    [Fact]
    public void SubmitAndContinue()
    {
        List<string> list = [];

        var order = new Order(() =>
        {
            list.Add("submit");
            list.Add("done");
            return true;
        }, out var complete);

        list.Add("start");

        order.ContinueWith(() =>
        {
            list.Add("continue");
        }).ContinueWith(() =>
        {
            list.Add("continue2");
        }).Submit();

        // list.Add("done");
        // complete.Invoke();

        list.Add("finish");

        Assert.Equal([
            "start",
            "submit",
            "done",
            "continue",
            "continue2",
            "finish"
        ], list);
    }

    // [Fact]
    // public async Task AwaitSubmit()
    // {
    //     List<string> list = [];
    //
    //     var order = new Order(() =>
    //     {
    //         list.Add("submit");
    //         return true;
    //     }, out var _);
    //
    //     list.Add("start");
    //
    //     await order.ContinueWith(() =>
    //     {
    //         list.Add("continue");
    //     }).SubmitForAwait();
    //
    //     list.Add("finish");
    //
    //     Assert.Equal([
    //         "start",
    //         "submit",
    //         "continue",
    //         "finish"
    //     ], list);
    // }
}