namespace KirisameY.Numeric.Test.NumericTests;

public class UpdatedEventTests
{
    private sealed class UpdatedRecorder
    {
        public int Count { get; private set; }

        public object? LastSender { get; private set; }

        public EventArgs? LastArgs { get; private set; }

        public void OnUpdated(object? sender, EventArgs e)
        {
            Count++;
            LastSender = sender;
            LastArgs   = e;
        }
    }

    [Fact]
    public void UpdatedIsRaisedWhenBaseValueIsSet()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.BaseValue = 20.0;

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void UpdatedIsRaisedForEveryBaseValueWrite()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.BaseValue = 20.0;
        numeric.BaseValue = 20.0;
        numeric.BaseValue = 30.0;

        Assert.Equal(3, recorder.Count);
    }

    [Fact]
    public void UpdatedIsNotRaisedByValueReads()
    {
        var numeric = NumericTestUtils.Create(10.0);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 5));
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        _ = numeric.Value;
        _ = numeric.Value;
        _ = numeric.BaseValue;

        Assert.Equal(0, recorder.Count);
    }

    [Fact]
    public void UpdatedIsRaisedWhenModifierIsAdded()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 5));

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void UpdatedIsRaisedWhenModifierIsRemoved()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 5);
        numeric.AddModifier(modifier);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        Assert.True(numeric.RemoveModifier(modifier));

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void UpdatedIsNotRaisedWhenRemoveModifierFails()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        Assert.False(numeric.RemoveModifier(new TestModifier(TestOrder.Middle, v => v)));

        Assert.Equal(0, recorder.Count);
    }

    [Fact]
    public void UpdatedIsRaisedWhenModifierRaisesUpdated()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        modifier.RaiseUpdated();

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void UpdatedIsRaisedOncePerModifierNotification()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        modifier.RaiseUpdated();
        modifier.RaiseUpdated();

        Assert.Equal(2, recorder.Count);
    }

    [Fact]
    public void RemovedModifierNoLongerRaisesUpdated()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        numeric.RemoveModifier(modifier);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        modifier.RaiseUpdated();

        Assert.Equal(0, recorder.Count);
    }

    [Fact]
    public void UpdatedSenderIsTheNumeric()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.BaseValue = 20.0;

        Assert.Same(numeric, recorder.LastSender);
    }

    [Fact]
    public void UpdatedCarriesEmptyEventArgs()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.BaseValue = 20.0;

        Assert.Same(EventArgs.Empty, recorder.LastArgs);
    }

    [Fact]
    public void UpdatedHandlerObservesTheNewValue()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var observed = double.NaN;
        numeric.Updated += (_, _) => observed = numeric.Value;

        numeric.BaseValue = 20.0;

        Assert.Equal(20.0, observed);
    }

    [Fact]
    public void UpdatedHandlerObservesRecomputedValueWhenModifierRaisesUpdated()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        Assert.Equal(11.0, numeric.Value);

        modifier.Transform = v => v + 5;
        var observed = double.NaN;
        numeric.Updated += (_, _) => observed = numeric.Value;

        modifier.RaiseUpdated();

        Assert.Equal(15.0, observed);
    }

    [Fact]
    public void UnsubscribedHandlerIsNoLongerCalled()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;
        numeric.BaseValue = 20.0;
        Assert.Equal(1, recorder.Count);

        numeric.Updated -= recorder.OnUpdated;
        numeric.BaseValue = 30.0;

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WrappedNumericUnsubscribedHandlerIsNoLongerCalled()
    {
        var numeric = NumericTestUtils.Create(7);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;
        numeric.BaseValue = 9;
        Assert.Equal(1, recorder.Count);

        numeric.Updated -= recorder.OnUpdated;
        numeric.BaseValue = 11;

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WrappedNumericUnsubscribingOneHandlerKeepsOthersSubscribed()
    {
        var numeric = NumericTestUtils.Create(7);
        var removed = new UpdatedRecorder();
        var kept = new UpdatedRecorder();
        numeric.Updated += removed.OnUpdated;
        numeric.Updated += kept.OnUpdated;

        numeric.Updated -= removed.OnUpdated;
        numeric.BaseValue = 9;

        Assert.Equal(0, removed.Count);
        Assert.Equal(1, kept.Count);
    }

    [Fact]
    public void ReadonlyNumericUnsubscribedHandlerIsNoLongerCalled()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 1));
        Assert.Equal(1, recorder.Count);

        numeric.Updated -= recorder.OnUpdated;
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 1));

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void ReadonlyWrappedNumericUnsubscribedHandlerIsNoLongerCalled()
    {
        var numeric = NumericTestUtils.CreateReadonly(7);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;
        modifier.RaiseUpdated();
        Assert.Equal(1, recorder.Count);

        numeric.Updated -= recorder.OnUpdated;
        modifier.RaiseUpdated();

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void WrappedNumericForwardsBaseValueUpdated()
    {
        var numeric = NumericTestUtils.Create(7);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.BaseValue = 9;

        Assert.Equal(1, recorder.Count);
        Assert.Equal(9, numeric.Value);
    }

    [Fact]
    public void WrappedNumericForwardsModifierUpdated()
    {
        var numeric = NumericTestUtils.Create(7);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        modifier.RaiseUpdated();

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void ReadonlyNumericForwardsUpdated()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 1));

        Assert.Equal(1, recorder.Count);
    }

    [Fact]
    public void ReadonlyWrappedNumericForwardsUpdated()
    {
        var numeric = NumericTestUtils.CreateReadonly(7);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        var recorder = new UpdatedRecorder();
        numeric.Updated += recorder.OnUpdated;

        modifier.RaiseUpdated();
        Assert.Equal(1, recorder.Count);

        numeric.RemoveModifier(modifier);
        Assert.Equal(2, recorder.Count);
    }
}
