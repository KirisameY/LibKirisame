using JetBrains.Annotations;

namespace KirisameY.Randomizer;

public static class RandomizerExtensions
{
    extension(IRandomizer randomizer)
    {
        //32
        [PublicAPI] public uint NextU32(uint min, uint max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
            return randomizer.NextU32(max - min) + min;
        }

        [PublicAPI] public int NextI32(int max)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(max, 0);
            return (int)randomizer.NextU32((uint)max);
        }

        [PublicAPI] public int NextI32(int min, int max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
            return unchecked((int)randomizer.NextU32((uint)(max - min)) + min);
        }

        //64
        [PublicAPI] public ulong NextU64(ulong min, ulong max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
            return randomizer.NextU64(max - min) + min;
        }

        [PublicAPI] public long NextI64(long max)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(max, 0);
            return (long)randomizer.NextU64((ulong)max);
        }

        [PublicAPI] public long NextI64(long min, long max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
            return unchecked((long)randomizer.NextU64((ulong)(max - min)) + min);
        }

        //Float
        [PublicAPI] public float NextSingle(float max) => randomizer.NextSingle() * max;

        [PublicAPI] public float NextSingle(float min, float max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
            return randomizer.NextSingle(max - min) + min;
        }

        //Double
        [PublicAPI] public double NextDouble(double max) => randomizer.NextDouble() * max;

        [PublicAPI] public double NextDouble(double min, double max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
            return randomizer.NextDouble(max - min) + min;
        }
    }
}