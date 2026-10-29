using System.Linq;

using KirisameY.Randomizer.RandomGenerators;

using Xunit;

namespace KirisameY.Randomizer.Test.RandomizerTests;

public class XorShiftStarGeneratorTests
{
    public static TheoryData<ulong, ulong[]> ReferenceSequences => new()
    {
        {
            1UL,
            [
                0x47E4CE4B896CDD1DUL,
                0x23E6B1F7C713B0C5UL,
                0x37130782D18F6EF2UL,
                0xF027721A28579090UL,
                0x4028C88C78C4B8AAUL
            ]
        },
        {
            0x9E3779B97F4A7C15UL,
            [
                0x0D83B3E29A21487AUL,
                0x6BF22468CC7011DDUL,
                0xB02FC9D94B6BBA92UL,
                0x3B1D1CA45E578555UL,
                0x9F346D2E924E92C4UL
            ]
        }
    };

    [Theory]
    [MemberData(nameof(ReferenceSequences))]
    public void ProducesTheReferenceSequenceForASeed(ulong seed, ulong[] expected)
    {
        var generator = new XorShiftStarGenerator(seed);

        var actual = new ulong[expected.Length];
        for (var i = 0; i < actual.Length; i++)
            actual[i] = generator.NextU64();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConstructorStoresTheInitialState()
    {
        var generator = new XorShiftStarGenerator(0x123456789ABCDEF0UL);

        Assert.Equal(0x123456789ABCDEF0UL, generator.State);
    }

    [Fact]
    public void StateTracksTheLastReturnedValue()
    {
        var generator = new XorShiftStarGenerator(1);

        for (var i = 0; i < 16; i++)
        {
            var value = generator.NextU64();
            Assert.Equal(value, generator.State);
        }
    }

    [Fact]
    public void RestoreReturnsANewGeneratorWithTheGivenState()
    {
        var generator = XorShiftStarGenerator.Restore(0xDEADBEEFUL);

        Assert.Equal(0xDEADBEEFUL, generator.State);
    }

    [Fact]
    public void RestoreContinuesTheExactSequence()
    {
        var original = new XorShiftStarGenerator(0x9E3779B97F4A7C15UL);
        for (var i = 0; i < 7; i++)
            original.NextU64();

        var restored = XorShiftStarGenerator.Restore(original.State);

        for (var i = 0; i < 16; i++)
            Assert.Equal(original.NextU64(), restored.NextU64());
    }

    [Fact]
    public void RestoreIsReachableThroughTheGenericInterface()
    {
        var generator = RestoreViaGenericInterface<XorShiftStarGenerator>(0xCAFEUL);

        Assert.Equal(0xCAFEUL, generator.State);
    }

    [Fact]
    public void RestoreIsReachableThroughTheNonGenericInterface()
    {
        var generator = RestoreViaNonGenericInterface<XorShiftStarGenerator>(0xCAFEUL);

        Assert.IsType<XorShiftStarGenerator>(generator);
        Assert.Equal(0xCAFEUL, generator.State);
    }

    [Fact]
    public void SameSeedProducesTheSameSequence()
    {
        var first = new XorShiftStarGenerator(0x123456789ABCDEF0UL);
        var second = new XorShiftStarGenerator(0x123456789ABCDEF0UL);

        for (var i = 0; i < 32; i++)
            Assert.Equal(first.NextU64(), second.NextU64());
    }

    [Fact]
    public void DifferentSeedsProduceDifferentSequences()
    {
        var first = new XorShiftStarGenerator(1);
        var second = new XorShiftStarGenerator(2);

        var firstSequence = new ulong[8];
        var secondSequence = new ulong[8];
        for (var i = 0; i < 8; i++)
        {
            firstSequence[i]  = first.NextU64();
            secondSequence[i] = second.NextU64();
        }

        Assert.NotEqual(firstSequence, secondSequence);
    }

    [Theory]
    [InlineData(1UL)]
    [InlineData(0x9E3779B97F4A7C15UL)]
    [InlineData(ulong.MaxValue)]
    public void NonZeroSeedNeverProducesZero(ulong seed)
    {
        var generator = new XorShiftStarGenerator(seed);

        for (var i = 0; i < 1000; i++)
            Assert.NotEqual(0UL, generator.NextU64());
    }

    [Fact]
    public void InheritedApiConsumesTheGeneratedUInt64Sequence()
    {
        var generator = new XorShiftStarGenerator(1);
        var expected = new XorShiftStarGenerator(1);

        Assert.Equal((uint)expected.NextU64(), generator.NextU32());
        Assert.Equal((uint)expected.NextU64(10), generator.NextU32(10));
        Assert.InRange(generator.NextI32(), 0, int.MaxValue);
        Assert.InRange(generator.NextDouble(), 0.0, 1.0);
    }

    private static T RestoreViaGenericInterface<T>(ulong state) where T : ISavableRandomizer<T> => T.Restore(state);

    private static ISavableRandomizer RestoreViaNonGenericInterface<T>(ulong state) where T : ISavableRandomizer => T.Restore(state);
}