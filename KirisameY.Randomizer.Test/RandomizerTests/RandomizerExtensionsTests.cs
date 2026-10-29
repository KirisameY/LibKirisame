using System;

using KirisameY.Randomizer.Test.TestDoubles;

using Xunit;

namespace KirisameY.Randomizer.Test.RandomizerTests;

public class RandomizerExtensionsTests
{
    [Theory]
    [InlineData(0UL, 5U)]
    [InlineData(4UL, 9U)]
    public void NextU32RangeMapsTheOffsetIntoTheRequestedInterval(ulong next, uint expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextU32(5, 10));
    }

    [Theory]
    [InlineData(5U, 5U)]
    [InlineData(6U, 5U)]
    public void NextU32RangeRejectsBoundsThatAreNotIncreasing(uint min, uint max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextU32(min, max));
    }

    [Theory]
    [InlineData(0UL, 0)]
    [InlineData(9UL, 9)]
    [InlineData(10UL, 0)]
    public void NextI32WithMaxStaysWithinZeroToMaxExclusive(ulong next, int expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextI32(10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NextI32WithNonPositiveMaxThrows(int max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextI32(max));
    }

    [Theory]
    [InlineData(0UL, -10)]
    [InlineData(10UL, 0)]
    [InlineData(19UL, 9)]
    public void NextI32RangeSupportsNegativeBounds(ulong next, int expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextI32(-10, 10));
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(6, 5)]
    public void NextI32RangeRejectsBoundsThatAreNotIncreasing(int min, int max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextI32(min, max));
    }

    [Theory]
    [InlineData(0UL, 100UL)]
    [InlineData(9UL, 109UL)]
    public void NextU64RangeMapsTheOffsetIntoTheRequestedInterval(ulong next, ulong expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextU64(100, 110));
    }

    [Theory]
    [InlineData(5UL, 5UL)]
    [InlineData(6UL, 5UL)]
    public void NextU64RangeRejectsBoundsThatAreNotIncreasing(ulong min, ulong max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextU64(min, max));
    }

    [Theory]
    [InlineData(0UL, 0L)]
    [InlineData(9UL, 9L)]
    [InlineData(10UL, 0L)]
    public void NextI64WithMaxStaysWithinZeroToMaxExclusive(ulong next, long expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextI64(10));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void NextI64WithNonPositiveMaxThrows(long max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextI64(max));
    }

    [Theory]
    [InlineData(0UL, -10L)]
    [InlineData(10UL, 0L)]
    [InlineData(19UL, 9L)]
    public void NextI64RangeSupportsNegativeBounds(ulong next, long expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextI64(-10, 10));
    }

    [Theory]
    [InlineData(5L, 5L)]
    [InlineData(6L, 5L)]
    public void NextI64RangeRejectsBoundsThatAreNotIncreasing(long min, long max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextI64(min, max));
    }

    [Fact]
    public void NextSingleWithMaxScalesTheUnitResult()
    {
        Assert.Equal(0f, new FixedRandomizer(0).NextSingle(10));

        var nearUpper = new FixedRandomizer(ulong.MaxValue).NextSingle(10);
        Assert.True(nearUpper > 9f);
        Assert.True(nearUpper < 10f);
    }

    [Fact]
    public void NextSingleRangeSupportsNegativeBounds()
    {
        Assert.Equal(-5f, new FixedRandomizer(0).NextSingle(-5, 5));

        var nearUpper = new FixedRandomizer(ulong.MaxValue).NextSingle(-5, 5);
        Assert.True(nearUpper > 4f);
        Assert.True(nearUpper < 5f);
    }

    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(2f, 1f)]
    public void NextSingleRangeRejectsBoundsThatAreNotIncreasing(float min, float max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextSingle(min, max));
    }

    [Fact]
    public void NextDoubleWithMaxScalesTheUnitResult()
    {
        Assert.Equal(0.0, new FixedRandomizer(0).NextDouble(10));

        var nearUpper = new FixedRandomizer(ulong.MaxValue).NextDouble(10);
        Assert.True(nearUpper > 9.0);
        Assert.True(nearUpper < 10.0);
    }

    [Fact]
    public void NextDoubleRangeSupportsNegativeBounds()
    {
        Assert.Equal(-5.0, new FixedRandomizer(0).NextDouble(-5, 5));

        var nearUpper = new FixedRandomizer(ulong.MaxValue).NextDouble(-5, 5);
        Assert.True(nearUpper > 4.0);
        Assert.True(nearUpper < 5.0);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(2.0, 1.0)]
    public void NextDoubleRangeRejectsBoundsThatAreNotIncreasing(double min, double max)
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextDouble(min, max));
    }

    [Fact]
    public void FloatingPointRangesNeverReachTheirUpperBound()
    {
        // Even the largest UInt64 draw must stay strictly below 1.0,
        // so scaled and offset ranges keep an exclusive upper bound.
        var randomizer = new FixedRandomizer(ulong.MaxValue);

        Assert.True(randomizer.NextSingle() < 1f);
        Assert.True(randomizer.NextDouble() < 1.0);
        Assert.True(randomizer.NextSingle(0, 10) < 10f);
        Assert.True(randomizer.NextDouble(0, 10) < 10.0);
        Assert.True(randomizer.NextSingle(-5, 5) < 5f);
        Assert.True(randomizer.NextDouble(-5, 5) < 5.0);
    }
}
