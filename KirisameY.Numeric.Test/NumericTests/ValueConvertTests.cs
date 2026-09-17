namespace KirisameY.Numeric.Test.NumericTests;

public class ValueConvertTests
{
    [Fact]
    public void CheckedModeIsTheDefault()
    {
        var numeric = NumericTestUtils.Create(10);

        Assert.Equal(10, numeric.Value);
    }

    [Fact]
    public void CheckedModeThrowsWhenModifierPushesValueOutOfRange()
    {
        var numeric = NumericTestUtils.Create<byte>(200, ValueConvertMode.Checked);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        Assert.Throws<OverflowException>(() => _ = numeric.Value);
    }

    [Fact]
    public void CheckedModeKeepsThrowingOnSubsequentReads()
    {
        var numeric = NumericTestUtils.Create<byte>(200);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        Assert.Throws<OverflowException>(() => _ = numeric.Value);
        Assert.Throws<OverflowException>(() => _ = numeric.Value);
    }

    [Fact]
    public void SaturatingModeClampsWhenValueOverflows()
    {
        var numeric = NumericTestUtils.Create<byte>(200, ValueConvertMode.Saturating);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        Assert.Equal((byte)255, numeric.Value);
    }

    [Fact]
    public void SaturatingModeClampsBelowMinimum()
    {
        var numeric = NumericTestUtils.Create<byte>(10, ValueConvertMode.Saturating);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v - 100));

        Assert.Equal((byte)0, numeric.Value);
    }

    [Fact]
    public void TruncatingModeDiscardsFractionalPart()
    {
        var numeric = NumericTestUtils.Create(10, ValueConvertMode.Truncating);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 0.9));

        Assert.Equal(10, numeric.Value);
    }

    [Fact]
    public void TruncatingModeDoesNotThrowOnOverflow()
    {
        var numeric = NumericTestUtils.Create<byte>(200, ValueConvertMode.Truncating);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        var exception = Record.Exception(() => _ = numeric.Value);

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(ValueConvertMode.Checked)]
    [InlineData(ValueConvertMode.Saturating)]
    [InlineData(ValueConvertMode.Truncating)]
    public void InRangeValuesRoundTripInEveryMode(ValueConvertMode mode)
    {
        var numeric = NumericTestUtils.Create(42, mode);

        Assert.Equal(42, numeric.BaseValue);
        Assert.Equal(42, numeric.Value);
    }

    [Theory]
    [InlineData(ValueConvertMode.Checked)]
    [InlineData(ValueConvertMode.Saturating)]
    [InlineData(ValueConvertMode.Truncating)]
    public void BaseValueWriteRoundTripsInEveryMode(ValueConvertMode mode)
    {
        var numeric = NumericTestUtils.Create(0, mode);

        numeric.BaseValue = 42;

        Assert.Equal(42, numeric.BaseValue);
    }

    [Fact]
    public void DoubleValuesBypassConversionEntirely()
    {
        var numeric = NumericTestUtils.Create(double.MaxValue);

        Assert.Equal(double.MaxValue, numeric.Value);
    }
}
