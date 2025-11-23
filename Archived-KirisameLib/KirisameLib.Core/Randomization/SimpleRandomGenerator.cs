namespace KirisameLib.Randomization;

public abstract class SimpleRandomGenerator : IRandomGenerator
{
    public abstract uint NextUint32();

    public int NextInt32()
    {
        var i = unchecked((int)NextUint32());
        if (i < 0) i = -(i + 1);
        return i;
    }


    public ulong NextUint64() => ((ulong)NextUint32() << 32) + NextUint32();

    public long NextInt64()
    {
        var l = unchecked((long)NextUint64());
        if (l < 0) l = -(l + 1);
        return l;
    }


    public double NextDouble() => (double)NextUint64() / ulong.MaxValue;

    public float NextSingle() => (float)NextUint32() / uint.MaxValue;
}