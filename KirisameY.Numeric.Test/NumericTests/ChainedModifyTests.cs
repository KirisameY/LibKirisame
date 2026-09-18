namespace KirisameY.Numeric.Test.NumericTests;

public class ChainedModifyTests
{
    private sealed class UpdatedRecorder
    {
        public int Count { get; private set; }

        public void OnUpdated(object? sender, EventArgs e) => Count++;
    }

    // ---------- WithUpdateHandler ----------

    [Fact]
    public void WithUpdateHandlerReturnsTheSameInstance()
    {
        var numeric = NumericTestUtils.Create(10.0);

        var returned = numeric.WithUpdateHandler((_, _) => { });

        Assert.Same(numeric, returned);
    }

    [Fact]
    public void WithUpdateHandlerSubscribesTheHandler()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();

        numeric.WithUpdateHandler(recorder.OnUpdated);
        numeric.BaseValue = 20.0;

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WithUpdateHandlerDoesNotInvokeTheHandlerEagerly()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();

        numeric.WithUpdateHandler(recorder.OnUpdated);

        Assert.Equal(0, recorder.Count);
    }

    [Fact]
    public void WithUpdateHandlerIsEquivalentToSubscribingDirectly()
    {
        var chained = NumericTestUtils.Create(10.0);
        var direct = NumericTestUtils.Create(10.0);
        var chainedRecorder = new UpdatedRecorder();
        var directRecorder = new UpdatedRecorder();

        chained.WithUpdateHandler(chainedRecorder.OnUpdated);
        direct.Updated += directRecorder.OnUpdated;
        chained.BaseValue = 20.0;
        direct.BaseValue = 20.0;

        Assert.Equal(1, chainedRecorder.Count);
        Assert.Equal(directRecorder.Count, chainedRecorder.Count);
    }

    [Fact]
    public void WithUpdateHandlerSubscribesEveryHandlerInTheChain()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var first = new UpdatedRecorder();
        var second = new UpdatedRecorder();

        var returned = numeric
            .WithUpdateHandler(first.OnUpdated)
            .WithUpdateHandler(second.OnUpdated);
        numeric.BaseValue = 20.0;

        Assert.Same(numeric, returned);
        Assert.Equal(1, first.Count);
        Assert.Equal(1, second.Count);
    }

    [Fact]
    public void WithUpdateHandlerResultStaysWritable()
    {
        var numeric = NumericTestUtils.Create(10.0);

        // 返回值静态类型仍是 TNumeric，没有被拓宽成 INumeric<TValue>，所以链式调用之后依旧能写 BaseValue。
        var returned = numeric.WithUpdateHandler((_, _) => { });
        returned.BaseValue = 42.0;

        Assert.Equal(42.0, returned.Value);
    }

    [Fact]
    public void WithUpdateHandlerResultCanBeUnsubscribed()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.WithUpdateHandler(recorder.OnUpdated);
        numeric.BaseValue = 20.0;
        Assert.Equal(1, recorder.Count);

        numeric.Updated -= recorder.OnUpdated;
        numeric.BaseValue = 30.0;

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WithUpdateHandlerWorksOnReadonlyNumeric()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        var recorder = new UpdatedRecorder();

        var returned = numeric.WithUpdateHandler(recorder.OnUpdated);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 1));

        Assert.Same(numeric, returned);
        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WithUpdateHandlerWorksOnWrappedNumeric()
    {
        var numeric = NumericTestUtils.Create(7);
        var recorder = new UpdatedRecorder();

        var returned = numeric.WithUpdateHandler(recorder.OnUpdated);
        numeric.BaseValue = 9;

        Assert.Same(numeric, returned);
        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WithUpdateHandlerWorksOnReadonlyWrappedNumeric()
    {
        var numeric = NumericTestUtils.CreateReadonly(7);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        var recorder = new UpdatedRecorder();

        var returned = numeric.WithUpdateHandler(recorder.OnUpdated);
        modifier.RaiseUpdated();

        Assert.Same(numeric, returned);
        Assert.Equal(1, recorder.Count);
    }

    // ---------- WithModifier ----------

    [Fact]
    public void WithModifierReturnsTheSameInstance()
    {
        var numeric = NumericTestUtils.Create(10.0);

        var returned = numeric.WithModifier(
            new TestModifier(TestOrder.Middle, v => v + 1)
        );

        Assert.Same(numeric, returned);
    }

    [Fact]
    public void WithModifierAddsTheModifierToTheCollection()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);

        numeric.WithModifier(modifier);

        Assert.Single(numeric.Modifiers);
        Assert.Contains(modifier, numeric.Modifiers);
    }

    [Fact]
    public void WithModifierAffectsTheValue()
    {
        var numeric = NumericTestUtils.Create(10.0);

        numeric.WithModifier(
            new TestModifier(TestOrder.Middle, v => v + 5)
        );

        Assert.Equal(15.0, numeric.Value);
    }

    [Fact]
    public void WithModifierRaisesUpdated()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.WithModifier(
            new TestModifier(TestOrder.Middle, v => v + 1)
        );

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WithModifierIsEquivalentToAddModifier()
    {
        var chained = NumericTestUtils.Create(0.0);
        var direct = NumericTestUtils.Create(0.0);

        chained.WithModifier(
            new TestModifier(TestOrder.Middle, v => v * 3)
        );
        direct.AddModifier(new TestModifier(TestOrder.Middle, v => v * 3));

        Assert.Equal(direct.Value, chained.Value);
        Assert.Equal(direct.Modifiers.Count, chained.Modifiers.Count);
    }

    [Fact]
    public void WithModifierChainAppliesModifiersInAscendingOrder()
    {
        var numeric = NumericTestUtils.Create(0.0);

        numeric
            .WithModifier(new TestModifier(TestOrder.Late, v => v * 10))
            .WithModifier(new TestModifier(TestOrder.Early, v => v + 1))
            .WithModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        // Early: 0 + 1 = 1, Middle: 1 * 2 = 2, Late: 2 * 10 = 20
        Assert.Equal(20.0, numeric.Value);
    }

    [Fact]
    public void WithModifierChainKeepsInsertionOrderWithinTheSameOrder()
    {
        var numeric = NumericTestUtils.Create(0.0);

        numeric
            .WithModifier(new TestModifier(TestOrder.Middle, v => v + 1))
            .WithModifier(new TestModifier(TestOrder.Middle, v => v * 3));

        // 链式书写的先后就是插入先后：(0 + 1) * 3 = 3，反过来会得到 1
        Assert.Equal(3.0, numeric.Value);
    }

    [Fact]
    public void WithModifierChainPropagatesModifierUpdates()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        var recorder = new UpdatedRecorder();

        numeric.WithModifier(modifier);
        numeric.WithUpdateHandler(recorder.OnUpdated);
        Assert.Equal(11.0, numeric.Value);

        modifier.Transform = v => v + 5;
        modifier.RaiseUpdated();

        Assert.Equal(1, recorder.Count);
        Assert.Equal(15.0, numeric.Value);
    }

    [Fact]
    public void WithModifierChainKeepsTheModifierRemovable()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 5);

        var returned = numeric.WithModifier(modifier);

        Assert.True(returned.RemoveModifier(modifier));
        Assert.Equal(10.0, returned.Value);
    }

    [Fact]
    public void WithModifierWorksOnReadonlyNumeric()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 5);

        var returned = numeric.WithModifier(modifier);

        Assert.Same(numeric, returned);
        Assert.Contains(modifier, returned.Modifiers);
        Assert.Equal(15.0, returned.Value);
    }

    [Fact]
    public void WithModifierWorksOnWrappedNumeric()
    {
        var numeric = NumericTestUtils.Create(7);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);

        var returned = numeric.WithModifier(modifier);

        Assert.Same(numeric, returned);
        Assert.Equal(8, returned.Value);
    }

    [Fact]
    public void WithModifierWorksOnReadonlyWrappedNumeric()
    {
        var numeric = NumericTestUtils.CreateReadonly(7);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);

        var returned = numeric.WithModifier(modifier);

        Assert.Same(numeric, returned);
        Assert.Equal(8, returned.Value);
    }

    // ---------- 两个拓展方法混用 ----------

    [Fact]
    public void ChainedCallsCanBeMixedAndKeepOneInstance()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();

        var returned = numeric
            .WithUpdateHandler(recorder.OnUpdated)
            .WithModifier(new TestModifier(TestOrder.Early, v => v * 2))
            .WithModifier(new TestModifier(TestOrder.Late, v => v + 1));

        Assert.Same(numeric, returned);
        // Early: 10 * 2 = 20, Late: 20 + 1 = 21
        Assert.Equal(21.0, returned.Value);
        Assert.Equal(2, returned.Modifiers.Count);

        // 两次 WithModifier 是在订阅之后发生的，各触发一次 Updated
        Assert.Equal(2, recorder.Count);

        returned.BaseValue = 5.0;

        // Early: 5 * 2 = 10, Late: 10 + 1 = 11；写 BaseValue 再触发一次
        Assert.Equal(11.0, returned.Value);
        Assert.Equal(3, recorder.Count);
    }

    [Fact]
    public void ChainedCallsOnWrappedNumericPreserveWrapperSemantics()
    {
        var numeric = NumericTestUtils.Create(7);
        var recorder = new UpdatedRecorder();

        var returned = numeric
            .WithUpdateHandler(recorder.OnUpdated)
            .WithModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        Assert.Equal(14, returned.Value);
        // WithModifier 在订阅之后发生，触发一次 Updated
        Assert.Equal(1, recorder.Count);

        returned.BaseValue = 10;

        Assert.Equal(10, returned.BaseValue);
        Assert.Equal(20, returned.Value);
        // 再加上写 BaseValue 的一次
        Assert.Equal(2, recorder.Count);
    }

    [Fact]
    public void ChainedCallsOnReadonlyWrappedNumericPreserveWrapperSemantics()
    {
        var numeric = NumericTestUtils.CreateReadonly(7);
        var recorder = new UpdatedRecorder();
        var modifier = new TestModifier(TestOrder.Middle, v => v + 3);

        var returned = numeric
            .WithUpdateHandler(recorder.OnUpdated)
            .WithModifier(modifier);

        Assert.Equal(10, returned.Value);
        // WithModifier 在订阅之后发生，触发一次 Updated
        Assert.Equal(1, recorder.Count);

        modifier.RaiseUpdated();

        Assert.Equal(2, recorder.Count);
    }
}
