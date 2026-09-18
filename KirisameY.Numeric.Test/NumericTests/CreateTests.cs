using KirisameY.GenericUtils;

namespace KirisameY.Numeric.Test.NumericTests;

public class CreateTests
{
    [Fact]
    public void ValueEqualsBaseValueWhenNoModifierIsAdded()
    {
        var numeric = NumericTestUtils.Create(10.0);

        Assert.Equal(10.0, numeric.BaseValue);
        Assert.Equal(10.0, numeric.Value);
    }

    [Fact]
    public void CreatedNumericHasNoModifiers()
    {
        var numeric = NumericTestUtils.Create(10.0);

        Assert.Empty(numeric.Modifiers);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.5)]
    [InlineData(-42.25)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public void InitialDoubleValueIsPreserved(double value)
    {
        var numeric = NumericTestUtils.Create(value);

        Assert.Equal(value, numeric.BaseValue);
        Assert.Equal(value, numeric.Value);
    }

    [Fact]
    public void OrderTypeArgumentCanBePassedExplicitly()
    {
        var numeric = INumeric.Create(3.0, TypeA.Of<TestOrder>());

        Assert.Equal(3.0, numeric.Value);
    }

    [Fact]
    public void OrderTypeArgumentCanBeOmitted()
    {
        var numeric = INumeric.Create<double, TestOrder>(3.0);

        Assert.Equal(3.0, numeric.Value);
    }

    [Fact]
    public void BaseValueCanBeWritten()
    {
        var numeric = NumericTestUtils.Create(10.0);

        numeric.BaseValue = 25.0;

        Assert.Equal(25.0, numeric.BaseValue);
    }

    [Fact]
    public void NonDoubleValueIsWrappedWithoutLoss()
    {
        var numeric = NumericTestUtils.Create(7);

        Assert.Equal(7, numeric.BaseValue);
        Assert.Equal(7, numeric.Value);
    }

    [Fact]
    public void NonDoubleBaseValueCanBeWritten()
    {
        var numeric = NumericTestUtils.Create(7);

        numeric.BaseValue = 13;

        Assert.Equal(13, numeric.BaseValue);
    }

    [Fact]
    public void WrappedNumericExposesModifiers()
    {
        var numeric = NumericTestUtils.Create(7);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);

        numeric.AddModifier(modifier);

        Assert.Single(numeric.Modifiers);
        Assert.Contains(modifier, numeric.Modifiers);
    }

    [Fact]
    public void WrappedNumericModifiersViewTracksLaterChanges()
    {
        var numeric = NumericTestUtils.Create(7);
        var view = numeric.Modifiers;
        Assert.Empty(view);

        var modifier = new TestModifier(TestOrder.Early, v => v);
        numeric.AddModifier(modifier);
        Assert.Single(view);

        numeric.RemoveModifier(modifier);

        Assert.Empty(view);
    }
}
