using JetBrains.Annotations;

namespace KirisameLib.Randomization;

public class RandomBelt<TGenerator>(TGenerator generator) : RandomBelt
    where TGenerator : IRandomGenerator
{
    public TGenerator Generator { get; } = generator;
    protected override IRandomGenerator Rng => Generator;
}

public abstract class RandomBelt
{
    //Core
    protected abstract IRandomGenerator Rng { get; }


    //Methods
    //Uint
    public uint NextUint() => Rng.NextUint32();

    public uint NextUint(uint min, uint max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
        return NextUint() % (max - min) + min;
    }

    public uint NextUint(uint max) => NextUint(0, max);

    //Int
    public int NextInt() => Rng.NextInt32();

    public int NextInt(int min, int max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
        return NextInt() % (max - min) + min;
    }

    public int NextInt(int max) => NextInt(0, max);

    //Ulong
    public ulong NextUlong() => Rng.NextUint64();

    public ulong NextUlong(ulong min, ulong max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
        return NextUlong() % (max - min) + min;
    }

    public ulong NextUlong(ulong max) => NextUlong(0, max);

    //Long
    public long NextLong() => Rng.NextInt64();

    public long NextLong(long min, long max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
        return NextLong() % (max - min) + min;
    }

    public long NextLong(long max) => NextLong(0, max);

    //Float
    public float NextFloat() => Rng.NextSingle();

    public float NextFloat(float min, float max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
        return NextFloat() * (max - min) + min;
    }

    public float NextFloat(float max) => NextFloat(0, max);

    //Double
    public double NextDouble() => Rng.NextDouble();

    public double NextDouble(double min, double max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);
        return NextDouble() * (max - min) + min;
    }

    public double NextDouble(double max) => NextDouble(0, max);

    //Bool
    public bool NextBool(double chance) => NextDouble() < chance;

    public bool NextBool() => NextBool(0.5);

    //Collection
    [Pure] [LinqTunnel] public IEnumerable<T> Shuffle<T>(IEnumerable<T> list) => list.OrderBy(_ => NextUint());

    public void ShuffleInPlace<T>(IList<T> list)
    {
        if (list.IsReadOnly) throw new ArgumentException("List is read-only.", nameof(list));
        var shuffle = Shuffle(list).ToArray();
        foreach (int i in Enumerable.Range(0, list.Count))
            list[i] = shuffle[i];
    }

    [Pure] [LinqTunnel] public IEnumerable<T> Draw<T>(IEnumerable<T> enumerable, int count) => Shuffle(enumerable).Take(count);

    [Pure] public T? Draw<T>(IEnumerable<T> enumerable) => Shuffle(enumerable).FirstOrDefault();
}