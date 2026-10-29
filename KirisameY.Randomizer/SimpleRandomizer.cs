namespace KirisameY.Randomizer;

public abstract class SimpleRandomizer : IRandomizer
{
    public uint NextU32() => (uint)NextU64();

    public uint NextU32(uint max) => (uint)NextU64(max);

    public int NextI32() => (int)(NextU32() & (uint.MaxValue >>> 1));


    public abstract ulong NextU64();

    public ulong NextU64(ulong max)
    {
        ArgumentOutOfRangeException.ThrowIfZero(max);
        return NextU64() % max;
    }

    public long NextI64() => (long)(NextU64() & (ulong.MaxValue >>> 1));


    public double NextDouble() => NextU64() * 5.4210108624275218E-20;

    public float NextSingle() => NextU64() * 5.4210107E-20f;
}