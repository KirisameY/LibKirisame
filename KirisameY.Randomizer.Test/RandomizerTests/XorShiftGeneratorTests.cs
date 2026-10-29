using System.Linq;

using KirisameY.Randomizer.RandomGenerators;

using Xunit;

namespace KirisameY.Randomizer.Test.RandomizerTests;

public class XorShiftGeneratorTests
{
    public static TheoryData<ulong, ulong[]> ReferenceSequences => new()
    {
        {
            1UL,
            [
                0x0000000040822041UL,
                0x100041060C011441UL,
                0x9B1E842F6E862629UL,
                0xF554F503555D8025UL,
                0x860C1FB090599265UL
            ]
        },
        {
            0x9E3779B97F4A7C15UL,
            [
                0xDC1B77AE0BF34DADUL,
                0x64F0EEB9026E6076UL,
                0x7B07CE91E5906136UL,
                0x305F050C368DCC74UL,
                0x2CEB16E0A1C54AECUL
            ]
        }
    };

    [Theory]
    [MemberData(nameof(ReferenceSequences))]
    public void ProducesTheReferenceSequenceForASeed(ulong seed, ulong[] expected)
    {
        var generator = new XorShiftGenerator(seed);

        var actual = new ulong[expected.Length];
        for (var i = 0; i < actual.Length; i++)
            actual[i] = generator.NextU64();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConstructorStoresTheInitialState()
    {
        var generator = new XorShiftGenerator(0x123456789ABCDEF0UL);

        Assert.Equal(0x123456789ABCDEF0UL, generator.State);
    }

    [Fact]
    public void StateTracksTheLastReturnedValue()
    {
        var generator = new XorShiftGenerator(1);

        for (var i = 0; i < 16; i++)
        {
            var value = generator.NextU64();
            Assert.Equal(value, generator.State);
        }
    }

    [Fact]
    public void RestoreReturnsANewGeneratorWithTheGivenState()
    {
        var generator = XorShiftGenerator.Restore(0xDEADBEEFUL);

        Assert.Equal(0xDEADBEEFUL, generator.State);
    }

    [Fact]
    public void RestoreContinuesTheExactSequence()
    {
        var original = new XorShiftGenerator(0x9E3779B97F4A7C15UL);
        for (var i = 0; i < 7; i++)
            original.NextU64();

        var restored = XorShiftGenerator.Restore(original.State);

        for (var i = 0; i < 16; i++)
            Assert.Equal(original.NextU64(), restored.NextU64());
    }

    [Fact]
    public void RestoreIsReachableThroughTheGenericInterface()
    {
        var generator = RestoreViaGenericInterface<XorShiftGenerator>(0xCAFEUL);

        Assert.Equal(0xCAFEUL, generator.State);
    }

    [Fact]
    public void RestoreIsReachableThroughTheNonGenericInterface()
    {
        var generator = RestoreViaNonGenericInterface<XorShiftGenerator>(0xCAFEUL);

        Assert.IsType<XorShiftGenerator>(generator);
        Assert.Equal(0xCAFEUL, generator.State);
    }

    [Fact]
    public void SameSeedProducesTheSameSequence()
    {
        var first = new XorShiftGenerator(0x123456789ABCDEF0UL);
        var second = new XorShiftGenerator(0x123456789ABCDEF0UL);

        for (var i = 0; i < 32; i++)
            Assert.Equal(first.NextU64(), second.NextU64());
    }

    [Fact]
    public void DifferentSeedsProduceDifferentSequences()
    {
        var first = new XorShiftGenerator(1);
        var second = new XorShiftGenerator(2);

        var firstSequence = new ulong[8];
        var secondSequence = new ulong[8];
        for (var i = 0; i < 8; i++)
        {
            firstSequence[i] = first.NextU64();
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
        var generator = new XorShiftGenerator(seed);

        for (var i = 0; i < 1000; i++)
            Assert.NotEqual(0UL, generator.NextU64());
    }

    [Fact]
    public void InheritedApiConsumesTheGeneratedUInt64Sequence()
    {
        var generator = new XorShiftGenerator(1);
        var expected = new XorShiftGenerator(1);

        Assert.Equal((uint)expected.NextU64(), generator.NextU32());
        Assert.Equal((uint)expected.NextU64(10), generator.NextU32(10));
        Assert.InRange(generator.NextI32(), 0, int.MaxValue);
        Assert.InRange(generator.NextDouble(), 0.0, 1.0);
    }

    private static T RestoreViaGenericInterface<T>(ulong state) where T : ISavableRandomizer<T> => T.Restore(state);

    private static ISavableRandomizer RestoreViaNonGenericInterface<T>(ulong state) where T : ISavableRandomizer => T.Restore(state);
}
