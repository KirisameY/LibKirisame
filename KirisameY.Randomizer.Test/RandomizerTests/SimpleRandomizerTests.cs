using System;

using KirisameY.Randomizer.Test.TestDoubles;

using Xunit;

namespace KirisameY.Randomizer.Test.RandomizerTests;

public class SimpleRandomizerTests
{
    [Theory]
    [InlineData(0UL, 0U)]
    [InlineData(0x0123456789ABCDEFUL, 0x89ABCDEFU)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL, uint.MaxValue)]
    [InlineData(0x1_0000_0000UL, 0U)]
    public void NextU32ReturnsTheLow32BitsOfNextU64(ulong next, uint expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextU32());
    }

    [Theory]
    [InlineData(10UL, 10U, 0U)]
    [InlineData(23UL, 10U, 3U)]
    [InlineData(4UL, uint.MaxValue, 4U)]
    public void NextU32WithMaxReturnsTheRemainderOfNextU64(ulong next, uint max, uint expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextU32(max));
    }

    [Fact]
    public void NextU32WithZeroMaxThrows()
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextU32(0));
    }

    [Theory]
    [InlineData(0UL, 0)]
    [InlineData(1UL, 1)]
    [InlineData(0x00000000FFFFFFFFUL, int.MaxValue)]
    [InlineData(0x0000000080000000UL, 0)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL, int.MaxValue)]
    [InlineData(0x000000007FFFFFFFUL, int.MaxValue)]
    public void NextI32MasksTheSignBit(ulong next, int expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextI32());
        Assert.InRange(randomizer.NextI32(), 0, int.MaxValue);
    }

    [Theory]
    [InlineData(0UL, 10UL, 0UL)]
    [InlineData(23UL, 10UL, 3UL)]
    [InlineData(ulong.MaxValue, 10UL, 5UL)]
    [InlineData(ulong.MaxValue, 1UL, 0UL)]
    public void NextU64WithMaxReturnsTheRemainderOfNextU64(ulong next, ulong max, ulong expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextU64(max));
        Assert.InRange(randomizer.NextU64(max), 0UL, max - 1);
    }

    [Fact]
    public void NextU64WithZeroMaxThrows()
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => randomizer.NextU64(0));
    }

    [Theory]
    [InlineData(0UL, 0L)]
    [InlineData(1UL, 1L)]
    [InlineData(0x8000000000000000UL, 0L)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL, long.MaxValue)]
    [InlineData(0x7FFFFFFFFFFFFFFFUL, long.MaxValue)]
    public void NextI64MasksTheSignBit(ulong next, long expected)
    {
        var randomizer = new FixedRandomizer(next);

        Assert.Equal(expected, randomizer.NextI64());
        Assert.InRange(randomizer.NextI64(), 0L, long.MaxValue);
    }

    [Fact]
    public void NextDoubleReturnsZeroWhenNextU64IsZero()
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Equal(0.0, randomizer.NextDouble());
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(0x8000000000000000UL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void NextDoubleStaysInTheUnitInterval(ulong next)
    {
        var randomizer = new FixedRandomizer(next);

        var value = randomizer.NextDouble();
        Assert.True(value >= 0.0);
        Assert.True(value < 1.0);
    }

    [Fact]
    public void NextSingleReturnsZeroWhenNextU64IsZero()
    {
        var randomizer = new FixedRandomizer(0);

        Assert.Equal(0f, randomizer.NextSingle());
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(0x8000000000000000UL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void NextSingleStaysInTheUnitInterval(ulong next)
    {
        var randomizer = new FixedRandomizer(next);

        var value = randomizer.NextSingle();
        Assert.True(value >= 0f);
        Assert.True(value < 1f);
    }

    [Fact]
    public void EveryConversionConsumesOneNextU64Call()
    {
        var randomizer = new FixedRandomizer(42);

        randomizer.NextU32();
        randomizer.NextU32(10);
        randomizer.NextI32();
        randomizer.NextU64(10);
        randomizer.NextI64();
        randomizer.NextDouble();
        randomizer.NextSingle();

        Assert.Equal(7, randomizer.NextU64CallCount);
    }
}
