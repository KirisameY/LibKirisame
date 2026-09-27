using KirisameY.BindingBridge.PropertyBinding;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     单向绑定（<c>OneWayPropertyBinding</c>）的行为，全部经由 <c>OneWayBindTo</c> 扩展方法进入。
/// </summary>
public class OneWayBindingTests
{
    private sealed class Holder
    {
        public int Number { get; set; }
        public string Text { get; set; } = "";
    }

    private sealed class OtherHolder
    {
        public string Text { get; set; } = "";
    }

    [Fact]
    public void OneWayBindPullsTheInitialValueIntoTheTarget()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        using var handle = from.OneWayBindTo(to, source, target);

        Assert.Equal(7, target.Number);
        Assert.Equal(1, to.SetCalls);
    }

    [Fact]
    public void OneWayBindRecordsExactlyOneSubscriptionOnTheSource()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        using var handle = from.OneWayBindTo(to, source, target);

        Assert.Equal(1, from.SubscribeCalls);
        Assert.Equal(1, from.HandlerCountOf(source));
        Assert.Equal(0, from.HandlerCountOf(new Holder()));
    }

    [Fact]
    public void OneWayBindPushesSourceUpdatesToTheTarget()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        using var handle = from.OneWayBindTo(to, source, target);

        source.Number = 9;
        from.RaiseUpdate(source);

        Assert.Equal(9, target.Number);
        Assert.Equal(2, to.SetCalls);
    }

    [Fact]
    public void OneWayBindDoesNotSubscribeToTheTarget()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeUniversalEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        using var handle = from.OneWayBindTo(to, source, target);

        Assert.Equal(0, to.SubscribeCalls);

        target.Number = 100;
        to.RaiseUpdate(target);

        // 单向就是单向：目标变化不回写
        Assert.Equal(7, source.Number);
    }

    [Fact]
    public void OneWayBindIgnoresUpdatesRaisedForOtherInstances()
    {
        var source = new Holder { Number = 7 };
        var other = new Holder { Number = 100 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        using var handle = from.OneWayBindTo(to, source, target);

        from.RaiseUpdate(other);

        Assert.Equal(7, target.Number);
        Assert.Equal(1, to.SetCalls);
    }

    [Fact]
    public void DisposingOneWayBindUnsubscribesTheSource()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        var handle = from.OneWayBindTo(to, source, target);

        handle.Dispose();

        Assert.Equal(1, from.UnsubscribeCalls);
        Assert.Equal(0, from.HandlerCountOf(source));
    }

    [Fact]
    public void DisposingOneWayBindStopsFurtherPropagation()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        var handle = from.OneWayBindTo(to, source, target);
        var setsBeforeDispose = to.SetCalls;

        handle.Dispose();
        source.Number = 9;
        from.RaiseUpdate(source);

        Assert.Equal(setsBeforeDispose, to.SetCalls);
        Assert.Equal(7, target.Number);
    }

    [Fact]
    public void DisposingOneWayBindTwiceUnsubscribesOnlyOnce()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        var handle = from.OneWayBindTo(to, source, target);

        handle.Dispose();
        handle.Dispose();

        Assert.Equal(1, from.UnsubscribeCalls);
    }

    [Fact]
    public void OneWayBindWithConverterAppliesItToTheInitialValueAndToUpdates()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, string>(h => h.Text, (h, v) => h.Text = v);

        using var handle = from.OneWayBindTo(to, source, target, v => $"#{v}");

        Assert.Equal("#7", target.Text);

        source.Number = 12;
        from.RaiseUpdate(source);

        Assert.Equal("#12", target.Text);
    }

    [Fact]
    public void OneWayBindSupportsDifferentSourceAndTargetObjectTypes()
    {
        var source = new Holder { Number = 7 };
        var target = new OtherHolder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<OtherHolder, string>(h => h.Text, (h, v) => h.Text = v);

        using var handle = from.OneWayBindTo(to, source, target, v => v.ToString());

        Assert.Equal("7", target.Text);

        source.Number = 3;
        from.RaiseUpdate(source);

        Assert.Equal("3", target.Text);
    }

    [Fact]
    public void OneWayBindWritesEveryUpdateInOrder()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = new FakeObservableEndpoint<Holder, int>(h => h.Number);
        var to = new FakeWritableEndpoint<Holder, int>(h => h.Number, (h, v) => h.Number = v);

        using var handle = from.OneWayBindTo(to, source, target);

        source.Number = 8;
        from.RaiseUpdate(source);
        source.Number = 9;
        from.RaiseUpdate(source);

        Assert.Equal(new[] { 7, 8, 9 }, to.Written);
    }
}
