using KirisameY.BindingBridge.PropertyBinding;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     双向绑定（<c>TwoWayPropertyBinding</c>）的行为，全部经由 <c>TwoWayBindTo</c> 扩展方法进入。
/// </summary>
public class TwoWayBindingTests
{
    private sealed class Holder
    {
        public int Number { get; set; }
        public string Text { get; set; } = "";
    }

    /// <summary>值不变就不写、也不发通知的持有者，用来观察双向绑定能否收敛。</summary>
    private sealed class GuardedHolder
    {
        private int _number;

        public int Number
        {
            get => _number;
            set
            {
                if (_number == value) return;
                _number = value;
            }
        }
    }

    private static FakeUniversalEndpoint<Holder, int> IntEndpoint() =>
        new(h => h.Number, (h, v) => h.Number = v);

    [Fact]
    public void TwoWayBindPullsTheInitialValueIntoTheTarget()
    {
        var source = new Holder { Number = 5 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = IntEndpoint();

        using var handle = from.TwoWayBindTo(to, source, target);

        Assert.Equal(5, target.Number);
        Assert.Equal(1, to.SetCalls);
    }

    [Fact]
    public void TwoWayBindSubscribesToBothEndpoints()
    {
        var source = new Holder { Number = 5 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = IntEndpoint();

        using var handle = from.TwoWayBindTo(to, source, target);

        Assert.Equal(1, from.SubscribeCalls);
        Assert.Equal(1, to.SubscribeCalls);
        Assert.Equal(1, from.HandlerCountOf(source));
        Assert.Equal(1, to.HandlerCountOf(target));
    }

    [Fact]
    public void TwoWayBindPushesSourceUpdatesToTheTarget()
    {
        var source = new Holder { Number = 5 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = IntEndpoint();

        using var handle = from.TwoWayBindTo(to, source, target);

        source.Number = 9;
        from.RaiseUpdate(source);

        Assert.Equal(9, target.Number);
    }

    [Fact]
    public void TwoWayBindPushesTargetUpdatesToTheSource()
    {
        var source = new Holder { Number = 5 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = IntEndpoint();

        using var handle = from.TwoWayBindTo(to, source, target);

        target.Number = 11;
        to.RaiseUpdate(target);

        Assert.Equal(11, source.Number);
    }

    [Fact]
    public void DisposingTwoWayBindUnsubscribesBothEndpoints()
    {
        var source = new Holder { Number = 5 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = IntEndpoint();

        var handle = from.TwoWayBindTo(to, source, target);

        handle.Dispose();

        Assert.Equal(1, from.UnsubscribeCalls);
        Assert.Equal(1, to.UnsubscribeCalls);
        Assert.Equal(0, from.HandlerCountOf(source));
        Assert.Equal(0, to.HandlerCountOf(target));
    }

    [Fact]
    public void DisposingTwoWayBindStopsPropagationInBothDirections()
    {
        var source = new Holder { Number = 5 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = IntEndpoint();

        var handle = from.TwoWayBindTo(to, source, target);

        handle.Dispose();

        source.Number = 9;
        from.RaiseUpdate(source);
        target.Number = 11;
        to.RaiseUpdate(target);

        Assert.Equal(9, source.Number);
        Assert.Equal(11, target.Number);
    }

    [Fact]
    public void DisposingTwoWayBindTwiceUnsubscribesEachEndpointOnlyOnce()
    {
        var source = new Holder { Number = 5 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = IntEndpoint();

        var handle = from.TwoWayBindTo(to, source, target);

        handle.Dispose();
        handle.Dispose();

        Assert.Equal(1, from.UnsubscribeCalls);
        Assert.Equal(1, to.UnsubscribeCalls);
    }

    [Fact]
    public void TwoWayBindWithConvertersAppliesEachDirectionSeparately()
    {
        var source = new Holder { Number = 7 };
        var target = new Holder();
        var from = IntEndpoint();
        var to = new FakeUniversalEndpoint<Holder, string>(h => h.Text, (h, v) => h.Text = v);

        using var handle = from.TwoWayBindTo(to, source, target, v => v.ToString(), s => int.Parse(s));

        // 正向转换器作用在初始拉取上
        Assert.Equal("7", target.Text);

        source.Number = 9;
        from.RaiseUpdate(source);
        Assert.Equal("9", target.Text);

        // 反向转换器负责目标到源
        target.Text = "42";
        to.RaiseUpdate(target);
        Assert.Equal(42, source.Number);
    }

    [Fact]
    public void TwoWayBindSettlesInOneRoundTripWhenTheSettersGuardOnEquality()
    {
        // AutoNotify 打开后，SetValue 引起的值变化会立刻回放通知。
        // 两端 Setter 都是"值不变就不写也不发通知"，因此双向绑定一轮之后收敛，而不是无限递归。
        var source = new GuardedHolder { Number = 5 };
        var target = new GuardedHolder();
        var from = new FakeUniversalEndpoint<GuardedHolder, int>(h => h.Number, (h, v) => h.Number = v) { AutoNotify = true };
        var to = new FakeUniversalEndpoint<GuardedHolder, int>(h => h.Number, (h, v) => h.Number = v) { AutoNotify = true };

        using var handle = from.TwoWayBindTo(to, source, target);

        // 初始拉取：source -> target，target 值变了于是回写 source，此时值已相同，收敛
        Assert.Equal(5, target.Number);
        Assert.Equal(1, to.SetCalls);
        Assert.Equal(1, from.SetCalls);

        source.Number = 9;
        from.RaiseUpdate(source);

        // 再走一轮：9 推到 target，回写 source 时值已相同
        Assert.Equal(9, target.Number);
        Assert.Equal(9, source.Number);
        Assert.Equal(2, to.SetCalls);
        Assert.Equal(2, from.SetCalls);
    }
}
