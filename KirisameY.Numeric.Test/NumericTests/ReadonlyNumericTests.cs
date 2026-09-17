namespace KirisameY.Numeric.Test.NumericTests;

public class ReadonlyNumericTests
{
    [Fact]
    public void ValueEqualsBaseValueWhenNoModifierIsAdded()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);

        Assert.Equal(10.0, numeric.BaseValue);
        Assert.Equal(10.0, numeric.Value);
    }

    [Fact]
    public void CreatedNumericHasNoModifiers()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);

        Assert.Empty(numeric.Modifiers);
    }

    [Fact]
    public void ModifierTransformsValue()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 5));

        Assert.Equal(15.0, numeric.Value);
        Assert.Equal(10.0, numeric.BaseValue);
    }

    [Fact]
    public void AddModifierExposesModifierInCollection()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v);

        numeric.AddModifier(modifier);

        Assert.Single(numeric.Modifiers);
        Assert.Contains(modifier, numeric.Modifiers);
    }

    [Fact]
    public void RemoveModifierReturnsTrueAndRemovesIt()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 5);
        numeric.AddModifier(modifier);

        Assert.True(numeric.RemoveModifier(modifier));

        Assert.Empty(numeric.Modifiers);
        Assert.Equal(10.0, numeric.Value);
    }

    [Fact]
    public void RemoveModifierReturnsFalseWhenModifierWasNeverAdded()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);

        Assert.False(numeric.RemoveModifier(new TestModifier(TestOrder.Middle, v => v)));
    }

    [Fact]
    public void ValueIsRecomputedWhenModifierRaisesUpdated()
    {
        var numeric = NumericTestUtils.CreateReadonly(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        Assert.Equal(11.0, numeric.Value);

        modifier.Transform = v => v + 5;
        modifier.RaiseUpdated();

        Assert.Equal(15.0, numeric.Value);
    }

    [Fact]
    public void ModifiersAreAppliedInAscendingOrder()
    {
        var numeric = NumericTestUtils.CreateReadonly(0.0);
        numeric.AddModifier(new TestModifier(TestOrder.Late, v => v * 10));
        numeric.AddModifier(new TestModifier(TestOrder.Early, v => v + 1));

        // Early: 0 + 1 = 1, Late: 1 * 10 = 10
        Assert.Equal(10.0, numeric.Value);
    }

    [Fact]
    public void NonDoubleValueIsWrappedWithoutLoss()
    {
        var numeric = NumericTestUtils.CreateReadonly(7);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 1));

        Assert.Equal(7, numeric.BaseValue);
        Assert.Equal(8, numeric.Value);
    }
}
